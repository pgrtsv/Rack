namespace Rack.CrossSectionUtils.Abstractions.Validation.Messages
{
    public interface ICrossSectionBuildSettingsValidationMessages
    {
        /// <summary>
        /// Сообщение о том, что масштаб по горизонтали должен быть больше нуля.
        /// </summary>
        string HorizontalScaleMustBeGreaterThanZero { get; }

        /// <summary>
        /// Сообщение о том, что масштаб по вертикали должен быть больше нуля.
        /// </summary>
        string VerticalScaleMustBeGreaterThanZero { get; }

        /// <summary>
        ///  Сообщение о том, что масштаб по горизонтали должен быть конечным числом. 
        /// </summary>
        string HorizontalScaleMustBeFinite { get; }

        /// <summary>
        ///  Сообщение о том, что масштаб по вертикали должен быть конечным числом. 
        /// </summary>
        string VerticalScaleMustBeFinite { get; }

        /// <summary>
        /// Сообщение о том, что разрешение по горизонтали должен быть больше нуля.
        /// </summary>
        string HorizontalResolutionMustBeGreaterThanZero { get; }

        /// <summary>
        /// Сообщение о том, что разрешение по вертикали должен быть больше нуля.
        /// </summary>
        string VerticalResolutionMustBeGreaterThanZero { get; }
        
        /// <summary>
        /// Сообщение о том, что верхняя граница разреза не может быть ниже нижней.
        /// </summary>
        string TopMustBeGreaterThanBottom { get; }

        /// <summary>
        /// Сообщение о том, что нижняя граница разреза не может быть выше верхней.
        /// </summary>
        string BottomMustBeLessThanTop { get; }

        /// <summary>
        /// Сообщение о том, что ширина колонок оформления должна быть не меньше 1 мм.
        /// </summary>
        string DecorationColumnsWidthMustBeGreaterThanOneMillimeter { get; }

        /// <summary>
        /// Сообщение о том, что высота заголовков колонок оформления должна быть не меньше 1 мм..
        /// </summary>
        string DecorationHeadersHeightMustBeGreaterThanOneMillimeter { get; }

        /// <summary>
        /// Сообщение о том, что расстояние отступа не может быть отрицательным числом.
        /// </summary>
        string OffsetMustBeGreaterThanZero { get; }
    }
}