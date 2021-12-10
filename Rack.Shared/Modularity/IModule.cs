namespace Rack.Shared.Modularity
{
    public interface IModule
    {
        string Name { get; }

        void RegisterTypes();

        void OnInitialized();
    }
}