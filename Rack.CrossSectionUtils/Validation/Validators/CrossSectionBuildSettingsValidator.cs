using FluentValidation;
using Rack.CrossSectionUtils.Abstractions.Model;
using Rack.CrossSectionUtils.Abstractions.Validation.Messages;

namespace Rack.CrossSectionUtils.Validation.Validators
{
    public class CrossSectionBuildSettingsValidator : AbstractValidator<ICrossSectionBuildSettings>
    {
        public CrossSectionBuildSettingsValidator(ICrossSectionBuildSettingsValidationMessages messages)
        {
            RuleFor(x => x.HorizontalScale)
                .GreaterThan(0)
                .WithMessage(messages.HorizontalScaleMustBeGreaterThanZero);
            RuleFor(x => x.HorizontalScale)
                .Must(double.IsFinite)
                .WithMessage(messages.HorizontalScaleMustBeFinite);

            RuleFor(x => x.VerticalScale)
                .GreaterThan(0)
                .WithMessage(messages.VerticalScaleMustBeGreaterThanZero);
            RuleFor(x => x.VerticalScale)
                .Must(double.IsFinite)
                .WithMessage(messages.VerticalScaleMustBeFinite);

            RuleFor(x => x.HorizontalResolution)
                .GreaterThan(0)
                .WithMessage(messages.HorizontalResolutionMustBeGreaterThanZero);
            
            RuleFor(x => x.VerticalResolution)
                .GreaterThan(0)
                .WithMessage(messages.VerticalResolutionMustBeGreaterThanZero);
            
            RuleFor(x => x.Top)
                .GreaterThan(x => x.Bottom)
                .WithMessage(messages.TopMustBeGreaterThanBottom);
            
            RuleFor(x => x.Bottom)
                .LessThan(x => x.Top)
                .WithMessage(messages.BottomMustBeLessThanTop);
            
            RuleFor(x => x.DecorationColumnsWidth.Millimeters)
                .GreaterThanOrEqualTo(1)
                .WithMessage(messages.DecorationColumnsWidthMustBeGreaterThanOneMillimeter);
            
            RuleFor(x => x.DecorationHeadersHeight.Millimeters)
                .GreaterThanOrEqualTo(1)
                .WithMessage(messages.DecorationHeadersHeightMustBeGreaterThanOneMillimeter);

            RuleFor(x => x.Offset.Millimeters)
                .GreaterThanOrEqualTo(0)
                .WithMessage(messages.OffsetMustBeGreaterThanZero);
        }
    }
}