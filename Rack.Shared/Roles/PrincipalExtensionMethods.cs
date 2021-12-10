using System;
using System.Linq;
using System.Security.Principal;

namespace Rack.Shared.Roles
{
    /// <summary>
    /// Режим проверки вхождения воли.
    /// </summary>
    [Obsolete]
    public enum RolesValidationMode
    {
        Any,
        All
    }

    public static class ExtensionMethods
    {
        /// <summary>
        /// Провереяет <see cref="principal" /> на соответствие одной или нескольким ролям <see cref="roles" />.
        /// </summary>
        /// <param name="principal">Проверяемый <see cref="IPrincipal" />.</param>
        /// <param name="roles">Роли, на вхождение в которые проверяется <see cref="principal" />.</param>
        /// <returns>true, если <see cref="principal" /> входит хотя бы в одну из <see cref="roles" />.</returns>
        public static bool IsInOneOfRoles(this IPrincipal principal, params string[] roles)
        {
            if (roles == null) throw new ArgumentNullException(nameof(roles));
            return roles.Aggregate(false, (current, otherRole) => current | principal.IsInRole(otherRole));
        }

        /// <summary>
        /// Провереяет <see cref="principal" /> на соответствие одной или нескольким ролям <see cref="roles" />.
        /// </summary>
        /// <param name="principal">Проверяемый <see cref="IPrincipal" />.</param>
        /// <param name="roles">Роли, на вхождение в которые проверяется <see cref="principal" />.</param>
        /// <returns>true, если <see cref="principal" /> входит во все <see cref="roles" />.</returns>
        public static bool IsInAllRoles(this IPrincipal principal, params string[] roles)
        {
            if (roles == null) throw new ArgumentNullException(nameof(roles));
            return roles.All(principal.IsInRole);
        }

        /// <summary>
        /// Провереяет <see cref="principal" /> на соответствие одной или нескольким ролям <see cref="roles" />.
        /// </summary>
        /// <param name="principal">Проверяемый <see cref="IPrincipal" />.</param>
        /// <param name="roles">Роли, на вхождение в которые проверяется <see cref="principal" />.</param>
        /// <returns>true, если <see cref="principal" /> входит хотя бы в одну из <see cref="roles" />.</returns>
        [Obsolete("Этот метод будет удалён. Используйте метод IsInOneOfRoles.")]
        public static bool IsInRoles(this IPrincipal principal, params string[] roles)
        {
            return IsInRoles(principal, RolesValidationMode.Any, roles);
        }

        /// <summary>
        /// Провереяет <see cref="principal" /> на соответствие одной или нескольким ролям <see cref="roles" />.
        /// </summary>
        /// <param name="principal">Проверяемый <see cref="IPrincipal" />.</param>
        /// <param name="mode">
        /// Режим проверки.
        /// В режиме <see cref="RolesValidationMode.All" /> метод вернёт true, если <see cref="principal" /> входит во все
        /// перечисленные
        /// <see cref="roles" />.
        /// В режиме <see cref="RolesValidationMode.Any" /> метод вернёт true, если <see cref="principal" /> входитхотя бы в одну
        /// из
        /// перечисленных <see cref="roles" />.
        /// </param>
        /// <param name="roles">Роли, на вхождение в которые проверяется <see cref="principal" />.</param>
        /// <returns>true, если <see cref="principal" /> входит в <see cref="roles" />.</returns>
        [Obsolete("Этот метод будет удалён. Используйте методы IsInOneOfRoles или IsInAllRoles.")]
        public static bool IsInRoles(this IPrincipal principal, RolesValidationMode mode,
            params string[] roles)
        {
            if (roles == null) throw new ArgumentNullException(nameof(roles));
            if (mode == RolesValidationMode.Any)
                return roles.Aggregate(false, (current, otherRole) => current | principal.IsInRole(otherRole));
            if (mode == RolesValidationMode.All)
                return roles.All(principal.IsInRole);
            throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
        }
    }
}