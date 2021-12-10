using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive;
using ReactiveUI;

namespace Rack.Shared
{
    public sealed class ReactiveMenuTab: ReactiveObject, IDisposable
    {
        public ReactiveMenuTab(IObservable<string> header, IEnumerable<ReactiveMenuTab> children)
        {
            _header = header.ToProperty(this, nameof(Header));
            Children = children.ToObservableCollection();
        }

        public ReactiveMenuTab(IObservable<string> header, ReactiveCommand<Unit, Unit> command)
        {
            _header = header.ToProperty(this, nameof(Header));
            Command = command;
        }

        private readonly ObservableAsPropertyHelper<string> _header;

        public string Header => _header.Value;

        public ObservableCollection<ReactiveMenuTab> Children { get; }

        public ReactiveCommand<Unit, Unit> Command { get; }

        public void Dispose()
        {
            _header.Dispose();
            Command?.Dispose();
        }
    }


}