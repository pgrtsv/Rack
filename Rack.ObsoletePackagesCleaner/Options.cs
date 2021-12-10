using CommandLine;

namespace Rack.ObsoletePackagesCleaner
{
    internal class Options
    {
        [Option('p', "path", Required = true, HelpText = "Set the directory path to clean up obsolete Nuget-packages.")]
        public string DeployPath { get; set; }
    }
}
