using FluentValidation;
using Rack.CrossSectionUtils.Abstractions.Model;
using Rack.CrossSectionUtils.Abstractions.Validation.Messages;

namespace Rack.CrossSectionUtils.Validation.Validators
{
    public class DecorationColumnRecordValidator<TDecorationColumnRecord> :
        AbstractValidator<TDecorationColumnRecord>
        where TDecorationColumnRecord : IDecorationColumnRecord
    {
        public DecorationColumnRecordValidator(
            IDecorationColumnRecordValidationMessages messages)
        {
            RuleFor(x => x.LeftBottom)
                .LessThan(x => x.LeftTop)
                .WithMessage(messages.LeftBottomMustBeLessThanLeftTop);
            RuleFor(x => x.LeftTop)
                .GreaterThan(x => x.LeftBottom)
                .WithMessage(messages.LeftTopMustBeGreaterThanLeftBottom);
            RuleFor(x => x.RightBottom)
                .LessThan(x => x.RightTop)
                .WithMessage(messages.RightBottomMustBeLessThanRightTop);
            RuleFor(x => x.RightTop)
                .GreaterThan(x => x.RightBottom)
                .WithMessage(messages.RightTopMustBeGreaterThanRightBottom);
        }
    }
}