using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Rack.LocalizationTool.Models.LocalizationData;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Rack.LocalizationTool.Models.ResolveOptions
{
    /// <summary>
    /// Параметры замены используемого несуществующего ключа-фразы.
    /// </summary>
    public class ReplaceNotExistedKeyOptions: ReactiveObject, IDisposable
    {
        private readonly CompositeDisposable _cleanUp = new CompositeDisposable();

        public ReplaceNotExistedKeyOptions(LocalizedPlace localizedPlace, string fileName)
        {
            LocalizedPlace = localizedPlace;
            FileName = fileName;
            this.WhenAnyValue(x => x.NewKey)
                .ObserveOnDispatcher()
                .Subscribe(x =>
                {
                    IsChecked = !string.IsNullOrEmpty(x);
                    IsCheckedEnable = !string.IsNullOrEmpty(x);
                }).DisposeWith(_cleanUp);
        }

        /// <summary>
        /// Место с локализацией в файле, где используются несуществующая ключ-фраза.
        /// </summary>
        public LocalizedPlace LocalizedPlace { get; }

        /// <summary>
        /// Имя файла.
        /// </summary>
        public string FileName { get; }

        /// <summary>
        /// Новый ключ.
        /// </summary>
        [Reactive]
        public string NewKey { get; set; }

        /// <summary>
        /// <see langword="true"/>, если необходимо применить замену несуществующего ключа.
        /// </summary>
        [Reactive]
        public bool IsChecked { get; set; }

        /// <summary>
        /// <see langword="true"/>, если можно изменять <see cref="IsChecked"/>.
        /// </summary>
        [Reactive]
        public bool IsCheckedEnable { get; private set; }

        public void Dispose()
        {
            _cleanUp?.Dispose();
        }
    }
}