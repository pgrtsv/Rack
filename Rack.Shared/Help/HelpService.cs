using System.Collections.Generic;

namespace Rack.Shared.Help
{
    public sealed class HelpService : IHelpService
    {
        private readonly List<HelpPage> _pages = new List<HelpPage>(10);
        public IReadOnlyCollection<HelpPage> Pages => _pages;

        /// <summary>
        /// Регистрирует страницу справки в сервисе.
        /// </summary>
        /// <param name="pageHeader">Заголовок страницы (локализованный).</param>
        /// <param name="pageContent">Содержимое страницы (локализованное, markdown).</param>
        /// <param name="moduleName">Название модуля (не локализованное).</param>
        /// <param name="language">Язык страницы.</param>
        public void RegisterPage(
            string pageHeader, 
            string pageContent, 
            string moduleName, 
            string language)
        {
            if (string.IsNullOrEmpty(moduleName)) moduleName = "Rack";
            _pages.Add(new HelpPage(pageHeader, pageContent, moduleName, language));
        }
    }
}