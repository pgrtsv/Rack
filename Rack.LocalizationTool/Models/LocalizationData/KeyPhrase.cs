using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using DynamicData;
using ReactiveUI;

namespace Rack.LocalizationTool.Models.LocalizationData
{
    /// <summary>
    /// Ключ, и все соответствующие ему локализации.
    /// </summary>
    public class KeyPhrase: ReactiveObject, IDisposable
    {
        private readonly SourceCache<LocalizationPhrase, string> _phrases;

        private readonly CompositeDisposable _cleanUp = new CompositeDisposable();

        public KeyPhrase(string key)
            : this(key, null)
        {}

        public KeyPhrase(string key, IEnumerable<LocalizationFile> localizationFiles)
        {
            Key = key;

            _phrases = new SourceCache<LocalizationPhrase, string>(
                    x => x.LocalizationFile.FilePath)
                .DisposeWith(_cleanUp);

            _phrases.Connect()
                .QueryWhenChanged()
                .ObserveOnDispatcher()
                .Subscribe(x => Phrases = x.Items.ToArray())
                .DisposeWith(_cleanUp);

            localizationFiles = localizationFiles?
                .Where(x => x.LocalizedValues.ContainsKey(key));
            if(localizationFiles != null) 
                _phrases.AddOrUpdate(
                    localizationFiles.Select(x => new LocalizationPhrase(this, x)));

        }
        /// <summary>
        /// Ключ локализации.
        /// </summary>
        public string Key { get; }

        /// <summary>
        /// Фразы ключа из всех локализаций.
        /// </summary>
        public IReadOnlyCollection<LocalizationPhrase> Phrases { get; private set; }

        /// <summary>
        /// Фразы ключа из всех локализаций.
        /// </summary>
        public IObservable<IChangeSet<LocalizationPhrase, string>> ConnectToPhrases => 
            _phrases.Connect();

        /// <summary>
        /// Локализации, в которых присутсвует ключ.
        /// </summary>
        public IEnumerable<LocalizationFile> GetLocalizations 
            => _phrases.Items.Select(x => x.LocalizationFile);

        /// <summary>
        /// true, если ключ присутствует хотя бы в одной локализации.
        /// </summary>
        public bool IsHasElements => _phrases.Count > 0;

        /// <summary>
        /// Обрабатывает локализацию: если ключ включён в локализацию – добавит локализацию в список;
        /// если эта локализация была добавлена, но теперь в ней нет ключа – удалит из списка локализаций.
        /// </summary>
        /// <param name="localizationFile">Файл локализации.</param>
        public void HandleLocalization(LocalizationFile localizationFile)
        {
            if (localizationFile.LocalizedValues.ContainsKey(Key))
                _phrases.AddOrUpdate(new LocalizationPhrase(this, localizationFile));
            else
                _phrases.Remove(localizationFile.FilePath);
        }

        public void Dispose()
        {
            _cleanUp?.Dispose();
        }
    }
}