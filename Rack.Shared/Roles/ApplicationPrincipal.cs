using System.Security.Principal;

namespace Rack.Shared.Roles
{
    /// <summary>
    /// Класс, предоставляющий доступ к текущему <see cref="IPrincipal" /> приложения через статическое свойство
    /// <see cref="Instance" />.
    /// </summary>
    public class ApplicationPrincipalFacade : IPrincipal
    {
        public static ApplicationPrincipalFacade Instance { get; } = new ApplicationPrincipalFacade();

        private ApplicationPrincipalFacade()
        {
        }

        public IPrincipal Value { get; set; }

        public bool IsInRole(string role)
        {
            return Value?.IsInRole(role) ?? false;
        }

        public IIdentity Identity => Value?.Identity;
    }
}