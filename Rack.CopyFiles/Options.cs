using System.Collections.Generic;
using CommandLine;

namespace Rack.CopyFiles
{
    public class Options
    {
        /// <summary>
        /// Путь к скомпилированным файлам модуля.
        /// </summary>
        [Option('m', "module", Required = true, HelpText = "Set path to module's bin/ folder.")]
        public string ModuleBinPath { get; set; }

        /// <summary>
        /// Путь к скомпилированным файлам оболочки.
        /// </summary>
        [Option('s', "shell", Required = true, HelpText = "Set path to shell's bin/ folder.")]
        public string ShellBinPath { get; set; }

        /// <summary>
        /// Имя главной DLL модуля.
        /// </summary>
        [Option("module-dll-name", Required = true, HelpText = "Set main (in module) DLL's name.")]
        public string ModuleDllName { get; set; }
    }
}