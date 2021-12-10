using System;

namespace Rack.Shared.DataAccess
{
    public interface IDatabaseNotificationService
    {
        IObservable<DatabaseNotification> Notifications { get; }

        IDisposable Connect();
    }
}