using System.Collections.ObjectModel;

namespace Rack.Shared
{
    /// <summary>
    /// Класс, содержащий команды уровня приложения в виде иерархии. Эти команды отображаются
    /// в главном меню приложения.
    /// </summary>
    public sealed class ApplicationTabs : ObservableCollection<ReactiveMenuTab>
    {
    }
}