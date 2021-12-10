using Rack.CrossSectionUtils.Abstractions.Validation.Messages;
using Rack.CrossSectionUtils.Wpf.Abstractions.Validation.Messages;
using Rack.Localization;

namespace Rack.CrossSectionUtils.Wpf.Validation.Messages
{
    /// <inheritdoc cref="IDecorationColumnRecordValidationMessages"/>
    public sealed class DecorationColumnRecordValidationMessages : ValidatorMessagesBase,
        IDecorationColumnRecordValidationMessages
    {
        public DecorationColumnRecordValidationMessages(
            ILocalizationService localizationService)
            : base(localizationService) { }

        /// <inheritdoc />
        public string LeftTopMustBeGreaterThanLeftBottom =>
            Localization["Validation.LeftTopMustBeGreaterThanLeftBottom"];

        /// <inheritdoc />
        public string LeftBottomMustBeLessThanLeftTop =>
            Localization["Validation.LeftBottomMustBeLessThanLeftTop"];

        /// <inheritdoc />
        public string RightTopMustBeGreaterThanRightBottom =>
            Localization["Validation.RightTopMustBeGreaterThanRightBottom"];

        /// <inheritdoc />
        public string RightBottomMustBeLessThanRightTop =>
            Localization["Validation.RightBottomMustBeLessThanRightTop"];
    }
}