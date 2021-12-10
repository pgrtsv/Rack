using System.Linq;
using FluentValidation;
using Rack.CrossSectionUtils.Abstractions.Model;
using Rack.CrossSectionUtils.Abstractions.Validation.Messages;

namespace Rack.CrossSectionUtils.Validation.Validators
{
    public class DecorationColumnWithRecordsValidator<TDecorationColumn, TDecorationColumnRecord> :
        AbstractValidator<TDecorationColumn>
        where TDecorationColumn: IDecorationColumnWithRecords<TDecorationColumnRecord>
        where TDecorationColumnRecord : IDecorationColumnRecord
    {
        public DecorationColumnWithRecordsValidator(
            IDecorationColumnWithRecordsValidationMessages messages,
            AbstractValidator<TDecorationColumn> baseValidator,
            AbstractValidator<TDecorationColumnRecord> recordValidator)
        {
            RuleFor(x => x)
                .SetValidator(baseValidator);

            RuleFor(x => x.Records)
                .ForEach(x => x.SetValidator(recordValidator));

            RuleFor(x => x.Records)
                .Must(records =>
                {
                    return records
                        .OrderByDescending(x => x.LeftTop)
                        .SkipLast(1)
                        .Zip(records.Skip(1),
                            (current, next) => current.LeftBottom >= next.LeftTop)
                        .All(x => x);
                }).WithMessage(messages.ConflictInLeftBorders);

            RuleFor(x => x.Records)
                .Must(records =>
                {
                    return records
                        .OrderByDescending(x => x.RightTop)
                        .SkipLast(1)
                        .Zip(records.Skip(1),
                            (current, next) => current.RightBottom >= next.RightTop)
                        .All(x => x);
                }).WithMessage(messages.ConflictInRightBorders);
        }
    }
}