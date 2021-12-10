using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using DynamicData;
using Rack.LocalizationTool.Models.LocalizationData;

namespace Rack.LocalizationTool.Services
{
    /// <summary>
    /// Сервис для обнаружения проблем, связанных с наличием у ключа разницы
    /// в количестве плейсхолдеров в фразах из разных локализаций.
    /// </summary>
    public class PhraseFormatDifferenceService: IDisposable
    {
        private readonly ProjectLocalizationData _localizationData;
        private readonly SourceCache<KeyPhrase, string> _stringFormatDifference;

        private readonly ChangeReason[] _handableReasons 
            = {ChangeReason.Add, ChangeReason.Update, ChangeReason.Refresh, ChangeReason.Update};

        private readonly BehaviorSubject<bool> _isInitialized;
        private readonly BehaviorSubject<bool> _isProblemsDetected;

        private readonly IDictionary<string, IDisposable> _keyPhrasesSubscriptions = 
            new Dictionary<string, IDisposable>();

        private readonly CompositeDisposable _cleanUp
            = new CompositeDisposable();

        public PhraseFormatDifferenceService(ProjectLocalizationData localizationData)
        {
            _localizationData = localizationData;
            _stringFormatDifference = new SourceCache<KeyPhrase, string>(
                    x => x.Key)
                .DisposeWith(_cleanUp);
            _isInitialized = new BehaviorSubject<bool>(false)
                .DisposeWith(_cleanUp);
            _isProblemsDetected = new BehaviorSubject<bool>(false)
                .DisposeWith(_cleanUp);
        }

        /// <summary>
        /// Ключи, фразы которых имеют разное количество плейсхолдеров в разных локализациях.
        /// </summary>
        public IObservable<IChangeSet<KeyPhrase, string>> ConnectToStringFormatDifference =>
            _stringFormatDifference.Connect();


        /// <summary>
        /// <see langword="true"/>, если сервис проинициализирован.
        /// </summary>
        public IObservable<bool> IsInitialized => _isInitialized;

        /// <summary>
        /// <see langword="true"/>, если сервис обнаружил проблемы.
        /// <see langword="true"/>, если проблем нет или сервис не проинициализирован.
        /// </summary>
        public IObservable<bool> IsProblemsDetected => _isProblemsDetected;

        /// <summary>
        /// Инициализирует сервис.
        /// </summary>
        public IObservable<Unit> Initialize(IScheduler mainScheduler)
            => Observable.Start(() =>
            {
                if (_isInitialized.FirstAsync().Wait()) return;

                _stringFormatDifference.CountChanged
                    .Subscribe(count => mainScheduler.Schedule(() =>
                        _isProblemsDetected.OnNext(count > 0)))
                    .DisposeWith(_cleanUp);

                _localizationData.ConnectToKeyPhrases()
                    .Subscribe(x =>
                    {
                        var handableChanges = x.Where(change => _handableReasons.Contains(change.Reason));
                        mainScheduler.Schedule(() =>
                        {
                            foreach (var change in handableChanges)
                            {
                                var keyPhrase = change.Current;
                                if (IsHasStringFormatDifference(keyPhrase))
                                    _stringFormatDifference.AddOrUpdate(keyPhrase);
                                else
                                    _stringFormatDifference.Remove(keyPhrase.Key);
                            }
                        });
                    })
                    .DisposeWith(_cleanUp);

                _isInitialized.OnNext(true);
            });

        public void Dispose()
        {
            _cleanUp.Dispose();
        }

        /// <summary>
        /// Проверяет, есть ли несогласованность в количестве плейсхолдеров у разных фраз одного ключа.
        /// </summary>
        /// <param name="keyPhrase">Ключ с фразами из разных локализаций.</param>
        /// <returns><see langword="true"/>, если есть несогласованность.</returns>
        public static bool IsHasStringFormatDifference(KeyPhrase keyPhrase)
        {
            return keyPhrase.Phrases
                       .Select(x => x.PlaceHolderCount)
                       .Distinct()
                       .Count() > 1;
        }
    }
}