using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Linq;
using CommandLine;

namespace Rack.CopyFiles
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
                        Console.WriteLine("Starting copying with following options:");
                        Console.WriteLine(JsonSerializer.Serialize(options, new JsonSerializerOptions
                        {
                            WriteIndented = true
                        }));
                        options.ModuleBinPath = options.ModuleBinPath.Trim();
                        options.ShellBinPath = options.ShellBinPath.Trim();
                        if (!Directory.Exists(options.ModuleBinPath))
                            throw new DirectoryNotFoundException(
                                $"Module's bin/ directory \"{options.ModuleBinPath}\" was not found.");
                        if (!Directory.Exists(options.ShellBinPath))
                            throw new DirectoryNotFoundException(
                                $"Shell's bin/ directory \"{options.ShellBinPath}\" was not found.");

                        // Проверяем название главной DLL модуля.
                        if (string.IsNullOrWhiteSpace(options.ModuleDllName))
                        {
                            options.ModuleDllName = Path.GetDirectoryName(options.ModuleBinPath);
                            while (!Path.GetFileName(options.ModuleDllName).Contains("Rack"))
                                options.ModuleDllName = Directory.GetParent(options.ModuleDllName).FullName;
                            options.ModuleDllName = Path.GetFileName(options.ModuleDllName);
                        }

                        if (string.IsNullOrWhiteSpace(options.ModuleDllName))
                            throw new ArgumentException("Module's main DLL's name is null or empty.");

                        if (!options.ModuleDllName.EndsWith(".dll"))
                            options.ModuleDllName += ".dll";

                        options.ModuleDllName = options.ModuleDllName.Trim();

                        var mainDll = Path.Combine(options.ModuleBinPath, options.ModuleDllName);
                        if (!File.Exists(mainDll))
                            throw new FileNotFoundException($"Module's main DLL \"{mainDll}\" not found.");

                        Console.WriteLine("Starting COPY:\n" +
                                          $"from \"{options.ModuleBinPath}\"\n" +
                                          $"to \"{options.ShellBinPath}\"\n" +
                                          $"with main DLL \"{options.ModuleDllName}\"\n");


                        // Если в папке оболочки не существует папки Modules, её необходимо создать.
                        if (!Directory.GetDirectories(options.ShellBinPath, "Modules").Any())
                            Directory.CreateDirectory(Path.Combine(options.ShellBinPath, "Modules"));

                        // Копируем файл модуля в папку Modules.
                        CopyNewerDll(mainDll, Path.Combine(options.ShellBinPath, "Modules"));
                    })
                    .WithNotParsed(errors =>
                    {
                        Environment.ExitCode = -1;
                        throw new ArgumentException("Не указаны необходимые параметры.");
                    });
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.Message);
                Console.WriteLine(exc.StackTrace);
                Console.WriteLine("Module's files copy failed!");
                Environment.ExitCode = -1;
                throw;
            }
        }

        public static void CopyNewerDll(string source, string destination)
        {
            var dllInfo = FileVersionInfo.GetVersionInfo(source);
            var dllName = Path.GetFileName(source);
            var existingDll = Path.Combine(destination, dllName);
            if (File.Exists(existingDll))
            {
                var existingDllInfo = FileVersionInfo.GetVersionInfo(existingDll);
                if (string.IsNullOrWhiteSpace(existingDllInfo.FileVersion))
                {
                    CopyNewerFile(source, destination);
                    return;
                }

                if (Version.Parse(existingDllInfo.FileVersion) < Version.Parse(dllInfo.FileVersion))
                {
                    File.Copy(source, existingDll, true);
                    Console.WriteLine($"COPY \"{source}\" to \"{destination}\": success, overriden by newer version.");
                    return;
                }

                if (Version.Parse(existingDllInfo.FileVersion) == Version.Parse(dllInfo.FileVersion))
                {
                    CopyNewerFile(source, destination);
                    return;
                }

                Console.WriteLine($"COPY \"{source}\" to \"{destination}\": missed, newer version is present.");
                return;
            }

            CopyNewerFile(source, destination);
        }

        public static void CopyNewerFile(string source, string destination)
        {
            var fileName = Path.GetFileName(source);
            var fileWriteTime = File.GetLastWriteTime(source);
            var existingFile = Path.Combine(destination, fileName);
            if (File.Exists(existingFile))
            {
                var existingFileWriteTime = File.GetLastWriteTime(existingFile);
                if (existingFileWriteTime < fileWriteTime)
                {
                    File.Copy(source, existingFile, true);
                    Console.WriteLine($"COPY \"{source}\" to \"{destination}\": success, overriden by newer version.");
                    return;
                }

                Console.WriteLine($"COPY \"{source}\" to \"{destination}\": missed, newer version is present.");
                return;
            }

            File.Copy(source, Path.Combine(destination, fileName));
            Console.WriteLine($"COPY \"{source}\" to \"{destination}\": success, new file.");
        }
    }
}