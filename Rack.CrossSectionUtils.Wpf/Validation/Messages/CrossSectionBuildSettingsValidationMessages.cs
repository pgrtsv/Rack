using Rack.CrossSectionUtils.Abstractions.Validation.Messages;
using Rack.CrossSectionUtils.Wpf.Abstractions.Validation.Messages;
using Rack.Localization;

namespace Rack.CrossSectionUtils.Wpf.Validation.Messages
{
    /// <inheritdoc cref="ICrossSectionBuildSettingsValidationMessages"/>
    public sealed class CrossSectionBuildSettingsValidationMessages
        : ValidatorMessagesBase, ICrossSectionBuildSettingsValidationMessages
    {
        public CrossSectionBuildSettingsValidationMessages(
            ILocalizationService localizationService)
            : base(localizationService) { }

        public string HorizontalScaleMustBeGreaterThanZero =>
            Localization["Validation.HorizontalScaleMustBeGreaterThanZero"];

        public string VerticalScaleMustBeGreaterThanZero =>
            Localization["Validation.VerticalScaleMustBeGreaterThanZero"];

        public string HorizontalScaleMustBeFinite =>
            Localization["Validation.HorizontalScaleMustBeFinite"];

        public string VerticalScaleMustBeFinite =>
            Localization["Validation.VerticalScaleMustBeFinite"];

        public string HorizontalResolutionMustBeGreaterThanZero =>
            Localization["Validation.HorizontalResolutionMustBeGreaterThanZero"];

        public string VerticalResolutionMustBeGreaterThanZero =>
            Localization["Validation.VerticalResolutionMustBeGreaterThanZero"];

        public string TopMustBeGreaterThanBottom =>
            Localization["Validation.TopMustBeGreaterThanBottom"];

        public string BottomMustBeLessThanTop =>
            Localization["Validation.BottomMustBeLessThanTop"];

        public string DecorationColumnsWidthMustBeGreaterThanOneMillimeter =>
            Localization["Validation.DecorationColumnsWidthMustBeGreaterThanOneMillimeter"];

        public string DecorationHeadersHeightMustBeGreaterThanOneMillimeter =>
            Localization["Validation.DecorationHeadersHeightMustBeGreaterThanOneMillimeter"];

        public string OffsetMustBeGreaterThanZero =>
            Localization["Validation.OffsetMustBeGreaterThanZero"];
    }
}