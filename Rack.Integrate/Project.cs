using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Rack.Integrate
{
    public class Project
    {
        public string Name { get; set; }

        public Guid SolutionGuid { get; set; }

        public string SolutionPath { get; set; }

        public string RelativePath { get; set; }

        public Guid Guid { get; set; }

        public List<string> Configurations { get; set; } = new List<string>();

        public bool IsMetaProject => !RelativePath.EndsWith(".csproj");

        protected bool Equals(Project other)
        {
            return Name.Equals(other.Name, StringComparison.OrdinalIgnoreCase);
        }

        public override string ToString()
        {
            return $"Project(\"{{{SolutionGuid}}}\") = \"{Name}\", \"{RelativePath}\", \"{{{Guid}}}\"";
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Project) obj);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        /// <summary>
        /// Инициализирует список проектов, которые указаны в решении.
        /// </summary>
        /// <param name="path">Путь к файлу решения.</param>
        public static List<Project> FromSolution(string path)
        {
            var projects = new List<Project>();
            var absolutePath = Directory.GetParent(Path.GetFullPath(path)).FullName;
            int linesCount = 0;
            using (var reader = File.OpenText(path))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    linesCount++;
                    if (line.StartsWith("Project"))
                        try
                        {
                            projects.Add(ParseProject(line, absolutePath));
                            continue;
                        }
                        catch
                        {
                            Console.WriteLine($"Error occured at line {linesCount} while parsing solution \"{path}\":");
                            Console.WriteLine(line);
                            throw;
                        }

                    if (line.Trim().StartsWith("GlobalSection(ProjectConfigurationPlatforms)"))
                    {
                        while (!reader.EndOfStream)
                        {
                            line = reader.ReadLine();
                            if (line.Trim().StartsWith("EndGlobalSection"))
                                break;
                            var projectGuid = Guid.Parse(line.Trim().Substring(1, 36));
                            projects.Find(x => x.Guid == projectGuid)
                                .Configurations
                                .Add(line);
                        }
                    }
                }
            }

            return projects;
        }

        /// <summary>
        /// Согласует данные проекта в соответствии с указанным решением (изменяет относительные пути).
        /// </summary>
        /// <param name="solutionPath">Путь к файлу решения.</param>
        /// <returns>Новый экземпляр проекта,
        /// данные которого согласованы с указанным решением.</returns>
        public Project ForAnotherSolution(string solutionPath)
        {
            return new Project
            {
                Guid = Guid,
                Name = Name,
                SolutionGuid = SolutionGuid,
                SolutionPath = solutionPath,
                RelativePath = new Uri(solutionPath)
                    .MakeRelativeUri(new Uri(Path.Combine(SolutionPath, RelativePath)))
                    .ToString()
            };
        }

        private static Project ParseProject(string line, string absolutePath)
        {
            var startIndex = 10;
            var solutionGuid = Guid.Parse(line.Substring(startIndex,
                line.IndexOf('}', startIndex) - startIndex));
            startIndex = line.IndexOf('=');
            var elements = line.Substring(startIndex + 1)
                .Split(',')
                .Select(x => x.Trim().Substring(1, x.Length - 3))
                .ToArray();
            return new Project
            {
                SolutionGuid = solutionGuid,
                Name = elements[0],
                SolutionPath = absolutePath,
                RelativePath = elements[1],
                Guid = Guid.Parse(elements[2].Substring(1, elements[2].Length - 2))
            };
        }
    }
}