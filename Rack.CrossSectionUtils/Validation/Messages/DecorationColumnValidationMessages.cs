using Rack.CrossSectionUtils.Abstractions.Validation.Messages;

namespace Rack.CrossSectionUtils.Validation.Messages
{
    public class DecorationColumnValidationMessages: 
        IDecorationColumnValidationMessages
    {
        /// <inheritdoc />
        public string HeaderShouldBeNotEmpty =>
            "Заголовок должен быть указан.";
    }
}