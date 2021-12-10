namespace Rack.CrossSectionUtils.Abstractions.Validation.Messages
{
    public interface IDecorationColumnValidationMessages
    {
        /// <summary>
        /// Сообщение о том, что заголовок должен быть указан.
        /// </summary>
        string HeaderShouldBeNotEmpty { get; }
    }
}