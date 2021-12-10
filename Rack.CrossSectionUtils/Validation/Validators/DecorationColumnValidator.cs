using FluentValidation;
using Rack.CrossSectionUtils.Abstractions.Model;
using Rack.CrossSectionUtils.Abstractions.Validation.Messages;

namespace Rack.CrossSectionUtils.Validation.Validators
{
    public class DecorationColumnValidator<TDecorationColumn> : AbstractValidator<TDecorationColumn>
        where TDecorationColumn: IDecorationColumn
    {
        /// <inheritdoc />
        public DecorationColumnValidator(IDecorationColumnValidationMessages messages)
        {
            RuleFor(x => x.Header)
                .NotEmpty()
                .WithMessage(messages.HeaderShouldBeNotEmpty);
        }
    }
}