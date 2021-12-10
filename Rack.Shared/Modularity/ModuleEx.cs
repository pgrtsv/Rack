using System;

namespace Rack.Shared.Modularity
{
    public static class ModuleEx
    {
        public static Version GetVersion(this IModule module) =>
            module.GetType().Assembly.GetName().Version;
    }
}