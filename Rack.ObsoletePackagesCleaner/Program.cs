using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CommandLine;

namespace Rack.ObsoletePackagesCleaner
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
                        Console.WriteLine(
                            $"Starting remove obsolete full NuPackage data at \"{options.DeployPath}\".");
                        
                        if (!Directory.Exists(options.DeployPath))
                            throw new DirectoryNotFoundException(
                                $"Directory \"{options.DeployPath}\" not exist.");

                        var releasesFilePath = $"{options.DeployPath}\\RELEASES";
                        var fullNupkgs = new DirectoryInfo(options.DeployPath)
                            .GetFiles("*full.nupkg");
                        if(fullNupkgs.Length == 0 || !File.Exists(releasesFilePath))
                            throw new ArgumentException($"\"{options.DeployPath}\" " +
                                                        $"is not deploy path with content created Squirrel.");

                        var actualFullNupkg = fullNupkgs.OrderBy(f => f.LastWriteTime).Last();
                        DeleteNotActualFiles(fullNupkgs, actualFullNupkg);
                        UpdateRealesesFile(releasesFilePath, actualFullNupkg.Name);
                    })
                    .WithNotParsed(errors =>
                    {
                        Environment.ExitCode = -1;
                        throw new ArgumentException("Required options are not provided.");
                    });
            }
            catch (Exception)
            {
                Console.WriteLine("Clearing obsolete full nuget-packages data failed!");
                Environment.ExitCode = -1;
                throw;
            }

            Console.WriteLine("Clearing obsolete full nuget-packages successfully completed.");
        }

        /// <summary>
        /// Удаляет все неактуальные Nuget-пакеты полной версии приложения (предыдущие версии).
        /// </summary>
        /// <param name="allFullNupkgs">Перечисление метаданных всех Nuget-пакетов полной версии приложения
        /// в директории публикации приложения.</param>
        /// <param name="actualFullNupkg">Метаданные актуального Nuget-пакета полной версии приложения.</param>
        public static void DeleteNotActualFiles(IEnumerable<FileInfo> allFullNupkgs, FileInfo actualFullNupkg)
        {
            foreach (var fileInfo in allFullNupkgs)
                if (fileInfo.LastWriteTime != actualFullNupkg.LastWriteTime)
                    File.Delete(fileInfo.FullName);
        }

        /// <summary>
        /// Обновляет файл "RELEASES": удаляет информацию о релизах
        /// не актуальных Nuget-пакетов полной версии приложения.
        /// </summary>
        /// <param name="releasesFilePath">Путь к файлу "RELEASES".</param>
        /// <param name="actualNupkgFileName">Имя актуального Nuget-пакета полной версии приложения.</param>
        public static void UpdateRealesesFile(string releasesFilePath, string actualNupkgFileName)
        {
            var actualContent = File.ReadAllLines(releasesFilePath);
            var newContent = new StringBuilder();
            var fileMask = new Regex("full");
            var actualMask = new Regex(actualNupkgFileName);
            foreach (var line in actualContent)
                if (!(fileMask.IsMatch(line) && !actualMask.IsMatch(line)))
                    newContent.AppendLine(line);

            File.WriteAllText(releasesFilePath, newContent.ToString());
        }
    }
}