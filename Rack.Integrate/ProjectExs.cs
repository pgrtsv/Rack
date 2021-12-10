using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Rack.Integrate
{
    public static class ProjectExs
    {
        /// <summary>
        /// Возвращает только те проекты, которые находятся
        /// в папке решения (были получены с репозитория).
        /// </summary>
        /// <param name="projects">Проекты решения (прочитанные из файла решения).</param>
        /// <param name="solutionFile">Путь к файлу решения.</param>
        public static IEnumerable<Project> GetOnlyPulledProjects(
            this IEnumerable<Project> projects,
            string solutionFile)
        {
            var absolutePath = Directory.GetParent(Path.GetFullPath(solutionFile)).FullName;

            var pulledProjects = Directory
                .GetFiles(absolutePath, "*.csproj", SearchOption.AllDirectories)
                .ToArray();

            return projects
                .Where(x => pulledProjects.Contains(Path.Combine(x.SolutionPath, x.RelativePath)));
        }
    }
}