using System.Collections.Generic;

namespace Rack.LocalizationTool.Models.LocalizationData
{
    /// <summary>
    /// Информация о локализации в файле.
    /// </summary>
    public sealed class LocalizedFileData
    {
        public LocalizedFileData(
            string filePath, 
            IReadOnlyCollection<LocalizedPlace> localizedPlaces)
        {
            FilePath = filePath;
            LocalizedPlaces = localizedPlaces;
        }

        /// <summary>
        /// Путь к файлу.
        /// </summary>
        public string FilePath { get; }

        /// <summary>
        /// Места в файле, где используется локализация.
        /// </summary>
        public IReadOnlyCollection<LocalizedPlace> LocalizedPlaces { get; }
    }
}