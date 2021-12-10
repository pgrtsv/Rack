using System.Threading.Tasks;

namespace Rack.Shared.DataAccess
{
    /// <summary>
    /// Делегат изменения сущности в БД.
    /// </summary>
    /// <param name="entityName">Название сущности (т. е. название типа соответствующего объекта в приложении).</param>
    /// <param name="statement">Тип действия, которое произвели с сущностью.</param>
    /// <param name="entity">Соответствующий сущности объект с обновлёнными данными.</param>
    public delegate Task EntitiesChangedHandler(string entityName, SqlStatement statement,
        object id);
}