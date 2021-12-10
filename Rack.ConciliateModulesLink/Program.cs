using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using CommandLine;
using Rack.Integrate;

namespace Rack.ConciliateModulesLink
{
    internal class Program
    {
        private const string RackNodePattern = @"Rack\.(\w+\.)*\w+";
        private const string RedundantModulePathPartPattern = RackNodePattern + @"\\Modules\\";

        private static readonly Regex ParentModuleRegex = new Regex(
            $"^\\s*<ProjectReference Include=\"(\\.\\.\\\\)+{RedundantModulePathPartPattern}({RackNodePattern}\\\\)*{RackNodePattern}.csproj\"\\s?\\/>");

        private static readonly Regex RedundantModulePathPartRegex = new Regex(RedundantModulePathPartPattern);
        private static readonly Regex OpenProjectReferenceTagRegex = new Regex("^\\s*<ProjectReference.*");
        private static readonly Regex EndTagRegex = new Regex(".*>\\s*$");

        private static void Main(string[] args)
        {
            try
            {
                Parser.Default.ParseArguments<Options>(args)
                    .WithParsed(options =>
                    {
                        Console.WriteLine("Starting conciliation modules link with following options:");
                        Console.WriteLine(JsonSerializer.Serialize(options, new JsonSerializerOptions
                        {
                            WriteIndented = true
                        }));

                        var moduleProjects = Project
                            .FromSolution(options.ModuleSolutionPath)
                            .Where(x => !x.IsMetaProject)
                            .GetOnlyPulledProjects(options.ModuleSolutionPath);

                        foreach (var moduleProject in moduleProjects)
                            ConciliateLinksToAnotherModulesFor(moduleProject);
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
                Console.WriteLine("Conciliation modules link failed!");
                Environment.ExitCode = -1;
                throw;
            }
        }

        private static void ConciliateLinksToAnotherModulesFor(Project project)
        {
            var projectAbsolutePath =
                Path.Combine(project.SolutionPath, project.RelativePath);
            var isContentUpdated = false;

            var projectContent = File.ReadAllLines(projectAbsolutePath);
            projectContent = MergeProjectReferenceLines(projectContent).ToArray();
            var newContent = projectContent.Select(line =>
            {
                if (!ParentModuleRegex.IsMatch(line))
                    return line;
                isContentUpdated = true;
                return RedundantModulePathPartRegex.Replace(line, "");
            }).ToArray();

            if (isContentUpdated)
                File.WriteAllLines(projectAbsolutePath, newContent);
        }

        private static IEnumerable<string> MergeProjectReferenceLines(IList<string> lines)
        {
            for (int i = 0; i < lines.Count; i++)
            {
                var line = lines[i];
                var isOpenTag = OpenProjectReferenceTagRegex.IsMatch(line);
                if (!isOpenTag || isOpenTag && EndTagRegex.IsMatch(line))
                {
                    yield return line;
                    continue;
                }

                while (!EndTagRegex.IsMatch(line))
                {
                    line = line.TrimEnd() + " " + lines[i + 1].Trim();
                    i++;
                }

                yield return line;
            }
        }
    }
}