using System;
using System.IO;
using System.Linq;
using System.Runtime.Loader;
using DryIoc;
using Rack.Shared.Modularity;

namespace Rack.Services
{
    public sealed class ModuleCatalog : IModuleCatalog
    {
        private readonly IContainer _container;

        public ModuleCatalog(IContainer container) => _container = container;

        public IModule[] Modules { get; private set; }

        public void LoadAndInitializeModules()
        {
            foreach (var moduleAssembly in Directory.EnumerateFiles(
                Environment.CurrentDirectory,
                "Rack.*.dll"))
                AssemblyLoadContext.Default.LoadFromAssemblyPath(Path.GetFullPath(moduleAssembly));
            var moduleTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .Where(x => x.GetInterfaces().Contains(typeof(IModule)))
                .ToArray();
            Modules = new IModule[moduleTypes.Length];
            for (int i = 0; i < moduleTypes.Length; i++)
            {
                var moduleType = moduleTypes[i];
                _container.Register(moduleType, Reuse.Transient);
                var module = _container.Resolve<IModule>(moduleType);
                module.RegisterTypes();
                module.OnInitialized();
                Modules[i] = module;
            }
        }
    }
}