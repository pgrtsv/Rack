using CommandLine;

namespace Rack.ConciliateModulesLink
{
    /// <summary>
    /// Опции (аргументы) передаваемые в консольное приложение. 
    /// </summary>
    public class Options
    {
        [Option('m', "module", Required = true, HelpText = "Set path to module's solution (sln) file.")]
        public string ModuleSolutionPath { get; set; }
    }
}