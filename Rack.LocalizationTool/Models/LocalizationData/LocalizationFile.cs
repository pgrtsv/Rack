using System.Collections.Generic;
using System.IO;
using Rack.Localization;

namespace Rack.LocalizationTool.Models.LocalizationData
{
    /// <summary>
    /// Файл локализации.
    /// </summary>
    public sealed class LocalizationFile
    {
        public LocalizationFile(string filePath, ILocalization localization)
        {
            FilePath = filePath;
            FileName = Path.GetFileName(filePath);
            Localization = localization;
        }

        /// <summary>
        /// Путь к файлу.
        /// </summary>
        public string FilePath { get; }

        /// <summary>
        /// Имя файла.
        /// </summary>
        public string FileName { get; }
        
        /// <summary>
        /// Локализация, содержащаяся в файле.
        /// </summary>
        public ILocalization Localization { get; set; }
        
        /// <summary>
        /// Ключ-фразы локализации.
        /// </summary>
        public IReadOnlyDictionary<string, string> LocalizedValues => Localization.LocalizedValues;
    }
}