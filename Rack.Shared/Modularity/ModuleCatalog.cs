namespace Rack.Shared.Modularity
{
    public interface IModuleCatalog
    {
        IModule[] Modules { get; }
        void LoadAndInitializeModules();
    }
}