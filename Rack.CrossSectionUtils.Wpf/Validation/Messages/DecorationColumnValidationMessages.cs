using Rack.CrossSectionUtils.Abstractions.Validation.Messages;
using Rack.CrossSectionUtils.Wpf.Abstractions.Validation.Messages;
using Rack.Localization;

namespace Rack.CrossSectionUtils.Wpf.Validation.Messages
{
    /// <inheritdoc cref="IDecorationColumnValidationMessages"/>
    public sealed class DecorationColumnValidationMessages : ValidatorMessagesBase,
        IDecorationColumnValidationMessages
    {
        public DecorationColumnValidationMessages(ILocalizationService localizationService)
            : base(localizationService) { }

        /// <inheritdoc />
        public string HeaderShouldBeNotEmpty =>
            Localization["Validation.HeaderShouldBeNotEmpty"];
    }
}