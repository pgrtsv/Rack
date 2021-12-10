using System.Collections.Generic;
using System.Linq;
using Rack.LocalizationTool.Models.LocalizationData;

namespace Rack.LocalizationTool.Models.LocalizationProblem
{
    /// <summary>
    /// Класс представляют проблему, когда в фале локализации отсутствуют ключи-фраз,
    /// присутствующие в других локализациях.
    /// </summary>
    public sealed class LocalizationAbsentKeys
    {
        public LocalizationAbsentKeys(
            LocalizationFile localizationFile,
            IEnumerable<string> absentKeys)
        {
            LocalizationFile = localizationFile;
            AbsentKeys = absentKeys.ToArray();
        }

        /// <summary>
        ///Файл локализации.
        /// </summary>
        public LocalizationFile LocalizationFile { get; }

        /// <summary>
        /// Список ключей-фраз, отсутствующих в файле локализации.
        /// </summary>
        public IList<string> AbsentKeys { get; }
    }
}