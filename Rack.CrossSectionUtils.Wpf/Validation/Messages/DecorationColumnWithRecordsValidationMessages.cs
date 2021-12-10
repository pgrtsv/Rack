using Rack.CrossSectionUtils.Abstractions.Validation.Messages;
using Rack.CrossSectionUtils.Wpf.Abstractions.Validation.Messages;
using Rack.Localization;

namespace Rack.CrossSectionUtils.Wpf.Validation.Messages
{
    /// <inheritdoc cref="IDecorationColumnWithRecordsValidationMessages"/>
    public sealed class DecorationColumnWithRecordsValidationMessages :
        ValidatorMessagesBase, IDecorationColumnWithRecordsValidationMessages
    {
        private readonly DecorationColumnValidationMessages _decorationColumnValidationMessages;

        public DecorationColumnWithRecordsValidationMessages(ILocalizationService localizationService)
            : base(localizationService)
        {
            _decorationColumnValidationMessages
                = new DecorationColumnValidationMessages(localizationService);
        }

        /// <inheritdoc />
        public string ConflictInLeftBorders =>
            Localization["Validation.ConflictInLeftBorders"];

        /// <inheritdoc />
        public string ConflictInRightBorders =>
            Localization["Validation.ConflictInRightBorders"];

        /// <inheritdoc />
        public string HeaderShouldBeNotEmpty =>
            _decorationColumnValidationMessages.HeaderShouldBeNotEmpty;
    }
}