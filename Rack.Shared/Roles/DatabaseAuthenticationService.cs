using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Security.Authentication;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Rack.Shared.Roles
{
    /// <summary>
    /// Класс, предоставляющий механизм аутентификации в БД.
    /// </summary>
    public sealed class DatabaseAuthenticationService :
        IDatabaseAuthenticationService,
        IDisposable
    {
        private readonly IConfiguration _configuration;
        private NpgsqlConnection _currentConnection;

        private readonly Subject<AuthentificationResult> _authenticated
            = new Subject<AuthentificationResult>();

        public DatabaseAuthenticationService(IConfiguration configuration) =>
            _configuration = configuration;

        public AuthentificationResult Authenticate(string username, string password) =>
            AuthenticateAsync(username, password).Result;

        public async Task<AuthentificationResult> AuthenticateAsync(
            string username,
            string password,
            CancellationToken cancellationToken = default)
        {
            var connectionString =
                new NpgsqlConnectionStringBuilder(_configuration["ConnectionString"])
                {
                    Username = username,
                    Password = password,
                    ApplicationName = "Rack",
                    Timeout = 30
                }.ConnectionString;

            var connection = new NpgsqlConnection(connectionString);

            var roles = new List<string>();

            try
            {
                await connection.OpenAsync(cancellationToken);
                if (cancellationToken.IsCancellationRequested) return null;
                await using (var getUserRoles = new NpgsqlCommand(
                    "SELECT get_user_roles()", connection))
                await using (var reader = getUserRoles.ExecuteReader())
                {
                    while (await reader.ReadAsync(cancellationToken))
                        roles.Add(reader.GetString(0));
                }

                PID = connection.ProcessID;
                connection.Close();
            }
            catch (PostgresException exc)
            {
                if (exc.SqlState == "28P01")
                    throw new AuthenticationException("Неверно указан логин или пароль.", exc);
                throw;
            }
            catch (NpgsqlException exc)
            {
                if (exc.Message.Contains("No password has been provided"))
                    throw new AuthenticationException("Необходимо указать пароль.", exc);
                throw new AuthenticationException("Произошла ошибка. Попробуйте снова.", exc);
            }

            CurrentConnection = connection;
            var result = new AuthentificationResult(
                new GenericPrincipal(new GenericIdentity(username), roles.ToArray()),
                connection);
            _authenticated.OnNext(result);
            return result;
        }

        public IObservable<AuthentificationResult> Authenticated => _authenticated;

        public NpgsqlConnection CurrentConnection
        {
            get => _currentConnection;
            set
            {
                if (_currentConnection != null)
                {
                    _currentConnection.Close();
                    _currentConnection.Dispose();
                }

                _currentConnection = value;
            }
        }

        public int PID { get; private set; }

        public void Dispose()
        {
            CurrentConnection = null;
        }
    }
}