using Rack.CrossSectionUtils.Abstractions.Validation.Messages;

namespace Rack.CrossSectionUtils.Validation.Messages
{
    public sealed class CrossSectionBuildSettingsValidationMessages
        : ICrossSectionBuildSettingsValidationMessages
    {
        public string HorizontalScaleMustBeGreaterThanZero =>
            "Масштаб по горизонтали должен быть больше нуля.";

        public string VerticalScaleMustBeGreaterThanZero =>
            "Масштаб по вертикали должен быть больше нуля.";

        public string HorizontalScaleMustBeFinite =>
            "Масштаб по горизонтали должен быть конечным числом.";

        public string VerticalScaleMustBeFinite =>
            "Масштаб по вертикали должен быть конечным числом.";

        public string HorizontalResolutionMustBeGreaterThanZero =>
            "Разрешение по горизонтали должно быть больше нуля.";

        public string VerticalResolutionMustBeGreaterThanZero =>
            "Разрешение по вертикали должно быть больше нуля.";

        public string TopMustBeGreaterThanBottom => 
            "Верхняя граница разреза не может быть ниже нижней.";

        public string BottomMustBeLessThanTop => 
            "Нижняя граница разреза не может быть выше верхней.";

        public string DecorationColumnsWidthMustBeGreaterThanOneMillimeter =>
            "Ширина колонок оформления должна быть не меньше 1 мм.";

        public string DecorationHeadersHeightMustBeGreaterThanOneMillimeter =>
            "Высота заголовков колонок оформления должна быть не меньше 1 мм.";

        public string OffsetMustBeGreaterThanZero =>
            "Расстояние отступа не может быть отрицательным числом.";
    }
}