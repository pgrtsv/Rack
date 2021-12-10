namespace Rack.GeoTools
{
    public interface ILabeledSpatial : ISpatial
    {
        /// <summary>
        /// Описание объекта, отображаемое в подписи на карте.
        /// </summary>
        string Description { get; }
    }
}