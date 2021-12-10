using CommandLine;

namespace Rack.Integrate
{
    public class Options
    {
        [Option('m', "module", Required = true, HelpText = "Set path to module's solution (sln) file.")]
        public string ModuleSolutionPath { get; set; }

        [Option('s', "shell", Required = true, HelpText = "Set path to shell's solution (sln) file.")]
        public string ShellSolutionPath { get; set; }
    }
}