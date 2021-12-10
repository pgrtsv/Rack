using System.Security.Principal;
using Npgsql;

namespace Rack.Shared.Roles
{
    public class AuthentificationResult
    {
        public AuthentificationResult(IPrincipal principal, NpgsqlConnection connection)
        {
            Principal = principal;
            Connection = connection;
        }

        public IPrincipal Principal { get; }

        public NpgsqlConnection Connection { get; }
    }
}