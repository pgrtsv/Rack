using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text.RegularExpressions;
using Rack.LocalizationTool.Models.LocalizationData;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Rack.LocalizationTool.Infrastructure
{
    /// <summary>
    /// Модель Представления для редактирования фразы по ключу.
    /// </summary>
    public class LocalizationPhraseViewModel: ReactiveObject, IDisposable
    {
        private LocalizationPhrase _source;
        /* Поскольку экземпляр фразы пересоздается при изменении файла локализации,
         * храним исходную фразу в отдельной переменной. */
        private string _sourcePhrase;

        private readonly Regex _countFormatArgumentsRegex = new Regex(@"{\d}");

        private readonly CompositeDisposable _cleanUp = new CompositeDisposable();

        private readonly BehaviorSubject<bool> _isPhraseHasUpdate;
        private readonly BehaviorSubject<bool> _isSetSourcePhrase;

        public LocalizationPhraseViewModel(LocalizationPhrase source)
        {
            _source = source;
            _sourcePhrase = source.Phrase;
            Phrase = source.Phrase;
            LocalizationFile = source.LocalizationFile;

            _isPhraseHasUpdate = new BehaviorSubject<bool>(false)
                .DisposeWith(_cleanUp);
            _isSetSourcePhrase = new BehaviorSubject<bool>(true)
                .DisposeWith(_cleanUp);

            this.WhenAnyValue(x => x.Phrase)
                .Throttle(TimeSpan.FromSeconds(1))
                .ObserveOnDispatcher()
                .Subscribe(x =>
                {
                    PlaceHolderCount = _countFormatArgumentsRegex.Matches(x).Count;
                    _isSetSourcePhrase.OnNext(x == _sourcePhrase);
                })
                .DisposeWith(_cleanUp);

            SetSourcePhrase = ReactiveCommand.Create(() =>
                {
                    Phrase = _source.Phrase;
                    _isPhraseHasUpdate.OnNext(false);
                }, _isSetSourcePhrase.Select(x => !x))
                .DisposeWith(_cleanUp);

            _isPhraseHasUpdate
                .ObserveOnDispatcher()
                .Subscribe(x => IsPhraseHasUpdate = x)
                .DisposeWith(_cleanUp);

            _isSetSourcePhrase
                .ObserveOnDispatcher()
                .Subscribe(x => IsSetSourcePhrase = x)
                .DisposeWith(_cleanUp);
        }

        /// <summary>
        /// Устанавливает в редактируемое свойство фразы <see cref="Phrase"/>,
        /// значение фразы в файле (актуальное на момент последнего обновления состояния экземпляра).
        /// </summary>
        public ReactiveCommand<Unit, Unit> SetSourcePhrase { get; }

        /// <summary>
        /// Свойство для хранения отредактированной фразы.
        /// </summary>
        [Reactive]
        public string Phrase { get; set; }

        /// <summary>
        /// Файл локализации, который содержит данную фразу.
        /// </summary>
        [Reactive]
        public LocalizationFile LocalizationFile { get; private set; }

        /// <summary>
        /// Количество плейсхолдеров в редактируемом свойстве фразы <see cref="Phrase"/>.
        /// </summary>
        [Reactive]
        public int PlaceHolderCount { get; private set; }

        /// <summary>
        /// Имеется ли обновление для фразы: <see langword="true"/>, если
        /// редактируемое свойство фразы изменено, а в это время изменилась фраза в файле.
        /// </summary>
        [Reactive]
        public bool IsPhraseHasUpdate { get; private set; }

        /// <summary>
        /// Редактируемое свойство фразы <see cref="Phrase"/> соответствует фразе в файле.
        /// </summary>
        [Reactive]
        public bool IsSetSourcePhrase { get; private set; }

        /// <summary>
        /// Количество плейсхолдеров в фразе в файле.
        /// </summary>
        [Reactive]
        public int SourcePlaceHolderCount { get; private set; }

        /// <summary>
        /// Обновляет состояние.
        /// </summary>
        /// <param name="source">Актуальная фраза.</param>
        public void UpdateSourcePhrase(LocalizationPhrase source)
        {
            if (_source.Key != source.Key || _source.LocalizationFile.FilePath != source.LocalizationFile.FilePath)
                throw new ArgumentException("Переданный аргумент не является обновленной исходной фразой.");
            _source = source;
            SourcePlaceHolderCount = source.PlaceHolderCount;
            LocalizationFile = source.LocalizationFile;
            if (_sourcePhrase == source.Phrase) return;
            _sourcePhrase = source.Phrase;
            _isPhraseHasUpdate.OnNext(true);
            _isSetSourcePhrase.OnNext(Phrase == _sourcePhrase);
        }

        public void Dispose()
        {
            _cleanUp?.Dispose();
        }
    }
}