using System;
using System.Reactive;

namespace Rack.Shared.Updates
{
    public interface IUpdateService
    {
        IObservable<bool> CanCheckForUpdates { get; }

        IObservable<bool> CheckForUpdates();

        IObservable<Unit> Update(IProgress<int> progress);
    }
}