using System.Collections.Generic;
using System.Linq;

namespace Rack.LocalizationTool.Models.LocalizationProblem
{
    /// <summary>
    /// Представляет проблемы в файле, связанные с несогласованностью количества аргументов
    /// в методе форматирования строки (которая использует локализацию) и количества плейсхолдеров
    /// в фразе (фразах) локализации.
    /// </summary>
    public class FormatArgumentsInconsistenciesAtFile
    {
        public FormatArgumentsInconsistenciesAtFile(IEnumerable<FormatArgumentsInconsistency> problems, 
            string filePath, string projectDirectory)
        {
            StringFormatProblems = problems.ToArray();
            FilePath = filePath;
            RelativePath = filePath.Replace(projectDirectory, "...");
        }

        /// <summary>
        /// Проблемы, обнаруженные в файле.
        /// </summary>
        public IReadOnlyCollection<FormatArgumentsInconsistency> StringFormatProblems { get; }

        /// <summary>
        /// Путь к файлу.
        /// </summary>
        public string FilePath { get; }

        /// <summary>
        /// Относительный путь к файлу.
        /// </summary>
        public string RelativePath { get; }
    }
}