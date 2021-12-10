using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using DynamicData;
using Rack.LocalizationTool.Models.LocalizationData;
using ReactiveUI;

namespace Rack.LocalizationTool.Models.LocalizationProblem
{
    /// <summary>
    /// Используемые ключи-фразы в файле проекта, которые отсутствуют в файлах локализации.
    /// </summary>
    public sealed class NotExistedKeys: ReactiveObject, IDisposable
    {
        private readonly CompositeDisposable _compositeDisposable = new CompositeDisposable();
        private readonly SourceList<LocalizedPlace> _localizedPlaces;
        
        public NotExistedKeys(
            string filePath, 
            IEnumerable<LocalizedPlace> localizationPlaces)
        {
            FilePath = filePath;
            _localizedPlaces = new SourceList<LocalizedPlace>()
                .DisposeWith(_compositeDisposable);
            _localizedPlaces.AddRange(localizationPlaces);
        }

        /// <summary>
        /// Имя файла.
        /// </summary>
        public string FilePath { get; }

        /// <summary>
        /// Участки кода, где используются ключи локализации, несуществующие ни в одной локализации.
        /// </summary>
        public IObservable<IChangeSet<LocalizedPlace>> ConnectToNotExistedKeyPlaces 
            => _localizedPlaces.Connect();

        /// <summary>
        /// Вернёт <see langword="true"/>, если ещё существуют проблемы.
        /// </summary>
        public bool IsContainsProblems() => _localizedPlaces.Count > 0;

        /// <summary>
        /// Обновляет информацию об используемых несуществующих ключах локализации.
        /// Актуально при изменении файлов, использующих локализацию
        /// </summary>
        /// <param name="localizedPlaces">Актуальные участки кода, где используются несуществующие ключи.</param>
        public void UpdateNotExistedKeys(IList<LocalizedPlace> localizedPlaces)
        {
            var deletedItems = _localizedPlaces.Items.Except(localizedPlaces);
            var newItems = localizedPlaces.Except(_localizedPlaces.Items);
            
            _localizedPlaces.Edit(list =>
            {
                foreach (var localizedPlace in deletedItems)
                    list.Remove(localizedPlace);
                list.AddRange(newItems);
            });
        }

        /// <summary>
        /// Добавляет информацию о использовании несуществующих ключей локализации.
        /// Актуально при изменении файлов локализации
        /// </summary>
        /// <param name="localizedPlaces">Участки кода в файле, где используются несуществующие ключи.</param>
        public void AddNotExistedKeys(IList<LocalizedPlace> localizedPlaces)
        {
            _localizedPlaces.Edit(list =>
            {
                foreach (var localizedPlace in localizedPlaces)
                    if(!_localizedPlaces.Items.Contains(localizedPlace))
                        _localizedPlaces.Add(localizedPlace);
            });
        }

        /// <summary>
        /// Обрабатывает новые ключи локализации: если какой-либо из новых ключей
        /// встречается в проблемах – эти проблемы удаляются.
        /// Актуально при изменении файлов локализации
        /// </summary>
        /// <param name="newKeys">Новые ключи локализации.</param>
        public void HandleAddingKeys(IEnumerable<string> newKeys)
        {
            foreach (var localizedPlace in _localizedPlaces.Items
                .Where(x => newKeys.Contains(x.LocalizationKey)))
                _localizedPlaces.Remove(localizedPlace);
        }

        public void Dispose()
        {
            _compositeDisposable?.Dispose();
        }
    }
}
