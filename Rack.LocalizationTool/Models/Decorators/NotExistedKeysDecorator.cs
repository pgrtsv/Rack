using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using DynamicData;
using Rack.LocalizationTool.Models.LocalizationProblem;
using Rack.LocalizationTool.Models.ResolveOptions;
using ReactiveUI;

namespace Rack.LocalizationTool.Models.Decorators
{
    /// <summary>
    /// Декоратор для используемых ключей-фраз в файле проекта,
    /// которые отсутствуют в файлах локализации.
    /// Направлен на инициализацию замен для несуществующих ключей.
    /// </summary>
    public class NotExistedKeysDecorator: ReactiveObject, IDisposable
    {
        private readonly CompositeDisposable _compositeDisposable = new CompositeDisposable();

        public NotExistedKeysDecorator(NotExistedKeys notExistedKeys, string projectDirectory)
        {
            FilePath = notExistedKeys.FilePath;
            RelativePath = FilePath.Replace(projectDirectory, "...");
            notExistedKeys.ConnectToNotExistedKeyPlaces
                .Transform(x => new ReplaceNotExistedKeyOptions(x, FilePath))
                .ObserveOnDispatcher()
                .Bind(out var options)
                .Subscribe()
                .DisposeWith(_compositeDisposable);
            NotExistedKeyOptions = options;
        }

        /// <summary>
        /// Путь к файлу.
        /// </summary>
        public string FilePath { get; }

        /// <summary>
        /// Относительный путь к файлу.
        /// </summary>
        public string RelativePath { get; }

        /// <summary>
        /// Опции для разрешения проблем с использованием несуществующих ключей.
        /// </summary>
        public IReadOnlyCollection<ReplaceNotExistedKeyOptions> NotExistedKeyOptions { get; }

        public void Dispose()
        {
            _compositeDisposable.Dispose();
            foreach (var option in NotExistedKeyOptions)
                option.Dispose();
        }
    }
}