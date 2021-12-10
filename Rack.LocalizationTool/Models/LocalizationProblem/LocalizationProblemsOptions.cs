using System.Linq;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Rack.LocalizationTool.Models.LocalizationProblem
{
    /// <summary>
    /// Опции анализа решения на проблемы с локализацией.
    /// </summary>
    public sealed class LocalizationProblemsOptions: ReactiveObject
    {
        /// <summary>
        /// <code>true</code> - если необходимо искать используемые ключи фразы,
        /// которые отсутствует в файлах локализации.
        /// </summary>
        [Reactive]
        public bool IsAnalyzeNotExistedKey { get; set; } = true;

        /// <summary>
        /// <code>true</code> - если необходимо искать в файлах локализации
        /// отсутствующие ключи фраз, которые присутствуют в других файлах локализации.
        /// </summary>
        [Reactive]
        public bool IsAnalyzeDifference { get; set; } = true;

        /// <summary>
        /// <code>true</code> - если необходимо искать ключи фразы из файла локализации,
        /// которые ни где не используется.
        /// </summary>
        [Reactive]
        public bool IsAnalyzeUnusedKey { get; set; } = true;

        /// <summary>
        /// Включить все опции.
        /// </summary>
        public void EnableAll()
        {
            var props = typeof(LocalizationProblemsOptions)
                .GetProperties()
                .Where(x => x.PropertyType == typeof(bool));
            foreach (var prop in props)
                prop.SetValue(this, true);
        }

        /// <summary>
        /// Выключить все опции.
        /// </summary>
        public void DisableAll()
        {
            var props = typeof(LocalizationProblemsOptions)
                .GetProperties()
                .Where(x => x.PropertyType == typeof(bool));
            foreach (var prop in props)
                prop.SetValue(this, false);
        }
    }
}