using System.Collections.Generic;
using System.Linq;
using Rack.LocalizationTool.Infrastructure;
using Rack.LocalizationTool.Models.LocalizationData;
using Rack.LocalizationTool.Models.LocalizationProblem;

namespace Rack.LocalizationTool.Models.Decorators
{
    /// <summary>
    /// Декоратор данных об отсутствующих в локализациях ключах-фраз.
    /// Позволяет проинициализировать отсутствующие ключи-фразы: указать фразы для ключей.
    /// </summary>
    public sealed class LocalizationAbsentKeysDecorator
    {
        public LocalizationAbsentKeysDecorator(LocalizationAbsentKeys localizationAbsentKeys)
        {
            LocalizationFile = localizationAbsentKeys.LocalizationFile;
            AbsentKeysPhrases = localizationAbsentKeys.AbsentKeys
                .Select(x =>new NewKeyPhraseViewModel(x))
                .ToArray();
        }

        /// <summary>
        /// Файл локалзиации.
        /// </summary>
        public LocalizationFile LocalizationFile { get; }

        /// <summary>
        /// Отсутсвующие ключи-фраз.
        /// </summary>
        public IReadOnlyCollection<NewKeyPhraseViewModel> AbsentKeysPhrases { get; }

        /// <summary>
        /// Формирует и возвращает перечисление отсутствующих ключей-фраз, для которых проинициализированы фразы.
        /// </summary>
        /// <returns>Перечисление ключей-фраз, для которых проинициализированы фразы.</returns>
        public IEnumerable<KeyValuePair<string, string>> GetResolvedAbsentKeysPhrases() => 
            AbsentKeysPhrases
                .Where(x => !string.IsNullOrEmpty(x.Phrase))
                .Select(x => new KeyValuePair<string,string>(x.Key, x.Phrase));
    }
}