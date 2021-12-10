using System;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;

namespace Rack.Shared.Roles
{
    public interface IDatabaseAuthenticationService
    {
        AuthentificationResult Authenticate(string username, string password);

        Task<AuthentificationResult> AuthenticateAsync(
            string username,
            string password,
            CancellationToken cancellationToken = default);

        IObservable<AuthentificationResult> Authenticated { get; }

        NpgsqlConnection CurrentConnection { get; }

        int PID { get; }
    }
}