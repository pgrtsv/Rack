using FluentValidation;
using Rack.CrossSectionUtils.Abstractions.Model;
using Rack.CrossSectionUtils.Validation.Messages;
using Rack.CrossSectionUtils.Validation.Validators;

namespace Rack.CrossSectionUtils.Model
{
    /// <inheritdoc />
    public class DecorationColumn: IDecorationColumn
    {
        public DecorationColumn(string header, DecorationColumnMode mode)
        {
            Header = header;
            Mode = mode;

            new DecorationColumnValidator<DecorationColumn>(new DecorationColumnValidationMessages())
                .ValidateAndThrow(this);
        }

        /// <inheritdoc />
        public string Header { get; }

        /// <inheritdoc />
        public DecorationColumnMode Mode { get; }
    }
}