using System.Collections.Generic;

namespace Rack.Shared.Help
{
    /// <summary>
    /// Сервис пользовательской справки.
    /// </summary>
    public interface IHelpService
    {
        IReadOnlyCollection<HelpPage> Pages { get; }

        void RegisterPage(string pageHeader, string pageContent, string moduleName, string language);
    }
}