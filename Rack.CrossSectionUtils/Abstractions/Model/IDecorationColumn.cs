using Rack.CrossSectionUtils.Model;

namespace Rack.CrossSectionUtils.Abstractions.Model
{
    /// <summary>
    /// Колонка оформления разреза.
    /// </summary>
    public interface IDecorationColumn
    {
        /// <summary>
        /// Заголовок колонки.
        /// </summary>
        string Header { get; }

        /// <summary>
        /// Режим отрисовки колонки.
        /// </summary>
        DecorationColumnMode Mode { get; }
    }
}