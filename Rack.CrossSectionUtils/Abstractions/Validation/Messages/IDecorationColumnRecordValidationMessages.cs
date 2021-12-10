namespace Rack.CrossSectionUtils.Abstractions.Validation.Messages
{
    public interface IDecorationColumnRecordValidationMessages
    {
        /// <summary>
        /// Сообщение о том, что левая верхняя граница должна быть выше левой нижней границы.
        /// </summary>
        string LeftTopMustBeGreaterThanLeftBottom { get; }

        /// <summary>
        /// Сообщение о том, что левая нижняя граница должна быть ниже левой верхней границы.
        /// </summary>
        string LeftBottomMustBeLessThanLeftTop { get; }

        /// <summary>
        /// Сообщение о том, что правая верхняя граница должна быть выше правая нижней границы.
        /// </summary>
        string RightTopMustBeGreaterThanRightBottom { get; }

        /// <summary>
        /// Сообщение о том, что правая нижняя граница должна быть ниже правой верхней границы.
        /// </summary>
        string RightBottomMustBeLessThanRightTop { get; }
    }
}