using Rack.CrossSectionUtils.Abstractions.Validation.Messages;

namespace Rack.CrossSectionUtils.Validation.Messages
{
    public class DecorationColumnRecordValidationMessages: 
        IDecorationColumnRecordValidationMessages
    {
        /// <inheritdoc />
        public string LeftTopMustBeGreaterThanLeftBottom =>
            "Левая верхняя граница должна быть выше левой нижней границы.";

        /// <inheritdoc />
        public string LeftBottomMustBeLessThanLeftTop =>
            "Левая нижняя граница должна быть ниже левой верхней границы.";

        /// <inheritdoc />
        public string RightTopMustBeGreaterThanRightBottom =>
            "Правая верхняя граница должна быть выше правой нижней границы.";

        /// <inheritdoc />
        public string RightBottomMustBeLessThanRightTop =>
            "Правая нижняя граница должна быть ниже правой верхней границы.";
    }
}