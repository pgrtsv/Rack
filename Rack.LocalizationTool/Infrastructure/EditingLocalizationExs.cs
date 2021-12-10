using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Rack.LocalizationTool.Models.LocalizationData;
using Rack.Localization;

namespace Rack.LocalizationTool.Infrastructure
{
    
    /// <summary>
    /// Класс содержит методы расшиерния для редактирования файла локализации.
    /// </summary>
    public static class EditingLocalizationExs
    {
        /// <summary>
        /// Обновляет содержание файла локализации: добовляет новые клюючи-фразы,
        /// обновляет фразы у существующих ключей.
        /// </summary>
        /// <param name="localizationFile">Файл локализации.</param>
        /// <param name="updateData">Ключи и фразы, которые необходимо добавить/обновить.</param>
        public static void AddOrUpdateKeysPhrases(
            this LocalizationFile localizationFile,
            IDictionary<string, string> updateData)
        {
            var newData = new Dictionary<string, string>(localizationFile.LocalizedValues);
            foreach (var (key, value) in updateData)
                if (!newData.ContainsKey(key))
                    newData.Add(key, value);
                else
                    newData[key] = value;
            localizationFile.RewriteKeysPhrases(newData);
        }

        /// <summary>
        /// Перезаписывает содержание файла локализации.
        /// </summary>
        /// <param name="localizationFile">Файл локализации.</param>
        /// <param name="keys">Новое содержание файла: ключи и фразы.</param>
        public static void RewriteKeysPhrases(
            this LocalizationFile localizationFile,
            IReadOnlyDictionary<string, string> keys)
        {
            keys = keys.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
            var localization = localizationFile.Localization;
            localization = new DefaultLocalization(localization.Language, localization.Key, keys);

            File.WriteAllText(
                localizationFile.FilePath,
                JsonConvert.SerializeObject(localization, Formatting.Indented),
                Encoding.UTF8);
        }
    }
}