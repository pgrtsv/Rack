using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Npgsql;
using Rack.Shared.Roles;

namespace Rack.Shared.DataAccess
{
    /// <summary>
    /// Класс, принимающий уведомления об изменениях в БД.
    /// </summary>
    public sealed class DatabaseNotificationListener : IDatabaseNotificationService, IDisposable
    {
        /// <summary>
        /// Прослушиваемый канал уведомлений.
        /// </summary>
        private const string NotificationChannel = "dml_messages";

        private readonly IDatabaseAuthenticationService _authenticationService;
        private readonly CompositeDisposable _cleanUp;
        private CompositeDisposable _mutableCleanUp;
        private NpgsqlConnection _connection;

        private readonly Subject<DatabaseNotification> _notifications
            = new Subject<DatabaseNotification>();

        public DatabaseNotificationListener(
            IDatabaseAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
            var connectOnAuthenticated = _authenticationService.Authenticated
                .Do(_ => Connect())
                .Subscribe();
            _cleanUp = new CompositeDisposable(_notifications,
                connectOnAuthenticated);
            _mutableCleanUp = new CompositeDisposable();
        }

        public IDisposable Connect()
        {
            _mutableCleanUp.Dispose();
            // NpgsqlConnection.WaitAsync(CancellationToken) не прекращает ожидание сразу после отмены, а лишь спустя какое-то время. 
            // Из-за этого попытка вызвать Dispose у этого соединения приведёт к InvalidOperationException, так как соединение будет в режиме ожидания.
            // Поэтому Dispose() вызываем с задержкой в 3 секунды.
            Observable.Return(_connection)
                .Where(x => x != null)
                .Delay(TimeSpan.FromSeconds(3))
                .Do(x => x.Dispose())
                .Subscribe();
            var connectionStringBuilder =
                new NpgsqlConnectionStringBuilder(
                    _authenticationService.CurrentConnection.ConnectionString);
            connectionStringBuilder.ApplicationName += " - listening notification channel";

            _connection = _authenticationService.CurrentConnection
                .CloneWith(connectionStringBuilder.ConnectionString);
            var eventListener = Observable
                .FromEventPattern<NotificationEventHandler, NpgsqlNotificationEventArgs>(
                    x => _connection.Notification += x,
                    x => _connection.Notification -= x)
                .Subscribe(pattern =>
                {
                    if (pattern.EventArgs.PID == _authenticationService.PID)
                        return;
                    var notification = JsonConvert.DeserializeObject<DatabaseNotification>(
                        pattern.EventArgs.Payload);
                    _notifications.OnNext(notification);
                });
            var connectionWaiter = Observable
                .StartAsync(async cancellationToken =>
                {
                    var connection = _connection;
                    await connection.OpenAsync(cancellationToken);
                    await using var listenCommand = new NpgsqlCommand(
                        $"LISTEN {NotificationChannel};", connection);
                    await listenCommand.ExecuteNonQueryAsync(cancellationToken);
                    while (!cancellationToken.IsCancellationRequested)
                        await connection.WaitAsync(cancellationToken);
                })
                .Subscribe();
            _mutableCleanUp = new CompositeDisposable(
                connectionWaiter,
                eventListener);

            return this;
        }

        public IObservable<DatabaseNotification> Notifications => _notifications;

        public void Dispose()
        {
            _cleanUp.Dispose();
            _mutableCleanUp.Dispose();
        }
    }
}