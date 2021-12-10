using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using CommandLine;

namespace Rack.Integrate
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            try
            {
                Parser.Default.ParseArguments<Options>(args)
                    .WithParsed(options =>
                    {
                        Console.WriteLine("Starting integration with following options:");
                        Console.WriteLine(JsonSerializer.Serialize(options, new JsonSerializerOptions
                        {
                            WriteIndented = true
                        }));
                        var moduleProjects = Project.FromSolution(options.ModuleSolutionPath);
                        var shellProjects = Project.FromSolution(options.ShellSolutionPath);
                        var newProjects = moduleProjects
                            .Except(shellProjects)
                            .Where(x => !x.IsMetaProject)
                            .GetOnlyPulledProjects(options.ModuleSolutionPath)
                            .ToList();
                        AddProjectsToSolution(options.ShellSolutionPath, newProjects);
                        var shellProject = shellProjects.Find(x => x.Name.Equals("Rack"));
                        foreach (var newProject in newProjects)
                            AddProjectReferenceToProject(newProject, shellProject);
                    })
                    .WithNotParsed(errors =>
                    {
                        Environment.ExitCode = -1;
                        throw new ArgumentException("Required options are not provided.");
                    });
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.Message);
                Console.WriteLine(exc.StackTrace);
                Console.WriteLine("Module's integration failed!");
                Environment.ExitCode = -1;
                throw;
            }
        }

        /// <summary>
        /// Добавляет в проект <see cref="project"/> ссылку на проект <see cref="referencedProject"/>. 
        /// </summary>
        /// <param name="referencedProject">Проект, на который указывается ссылка.</param>
        /// <param name="project">Проект, в который добавляется ссылка.</param>
        private static void AddProjectReferenceToProject(Project referencedProject, Project project)
        {
            var projectAbsolutePath =
                Path.Combine(project.SolutionPath, project.RelativePath);
            var referencedProjectAbsolutePath = Path.Combine(referencedProject.SolutionPath,
                referencedProject.RelativePath);
            var projectContent = File.ReadAllLines(projectAbsolutePath).ToList();
            var indexOfFirstProjectReference = projectContent.FindIndex(x => x.Contains("<ProjectReference"));
            projectContent.Insert(indexOfFirstProjectReference,
                $"<ProjectReference Include=\"{new Uri(projectAbsolutePath).MakeRelativeUri(new Uri(referencedProjectAbsolutePath))}\">");
            projectContent.Insert(indexOfFirstProjectReference + 1, $"<Project>{{{referencedProject.Guid}}}</Project>");
            projectContent.Insert(indexOfFirstProjectReference + 2, $"<Name>{referencedProject.Name}</Name>");
            projectContent.Insert(indexOfFirstProjectReference + 3, "</ProjectReference>");
            File.WriteAllLines(projectAbsolutePath, projectContent);
        }

        /// <summary>
        /// Добавляет в файл решения ссылки на проекты.
        /// </summary>
        /// <param name="solution">Путь к файлу решения.</param>
        /// <param name="projects">Проекты, на которые необходимо добавить ссылки.</param>
        private static void AddProjectsToSolution(string solution, IEnumerable<Project> projects)
        {
            var solutionContent = File.ReadAllLines(solution).ToList();
            var indexOfGlobalLine = solutionContent.FindIndex(x => x.Equals("Global"));
            foreach (var project in projects.Select(x => x.ForAnotherSolution(solution)))
            {
                solutionContent.Insert(indexOfGlobalLine - 1, project.ToString());
                solutionContent.Insert(indexOfGlobalLine - 1, "EndProject");
            }

            var indexOfProjectConfigurationLine = solutionContent.FindIndex(x =>
                x.Trim().StartsWith("GlobalSection(ProjectConfigurationPlatforms)"));
            foreach (var project in projects)
                foreach (var configuration in project.Configurations)
                    solutionContent.Insert(indexOfProjectConfigurationLine + 1, configuration);
            File.WriteAllLines(solution, solutionContent);
        }
    }
}