using System;
using System.Reactive.Linq;
using ReactiveUI;

namespace Rack.Shared
{
    // Some useful extensions to turn activatable objects into an IObservable<bool>.
    public static class ReactiveUiEx
    {
        public static IObservable<bool> GetIsActivated(this IActivatableViewModel @this) =>
            Observable
                .Merge(
                    @this.Activator.Activated.Select(_ => true),
                    @this.Activator.Deactivated.Select(_ => false))
                .Replay(1)
                .RefCount();
    }
}