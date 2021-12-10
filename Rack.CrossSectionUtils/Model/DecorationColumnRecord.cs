using Rack.CrossSectionUtils.Abstractions.Model;
using UnitsNet;
using UnitsNet.Units;

namespace Rack.CrossSectionUtils.Model
{
    /// <inheritdoc cref="IDecorationColumnRecord"/>
    public class DecorationColumnRecord : IDecorationColumnRecord
    {
        public DecorationColumnRecord(
            string text,
            double leftBottom,
            double leftTop,
            double rightBottom,
            double rightTop,
            LengthUnit lengthUnit = LengthUnit.Meter) :
            this(text,
                new Length(leftBottom, lengthUnit),
                new Length(leftTop, lengthUnit),
                new Length(rightBottom, lengthUnit),
                new Length(rightTop, lengthUnit))
        {}

        /// <inheritdoc />
        public DecorationColumnRecord(
            string text,
            Length leftBottom,
            Length leftTop,
            Length rightBottom,
            Length rightTop)
        {
            Text = text;
            LeftBottom = leftBottom;
            LeftTop = leftTop;
            RightBottom = rightBottom;
            RightTop = rightTop;
        }

        /// <inheritdoc />
        public string Text { get; }

        /// <inheritdoc />
        public Length LeftBottom { get; }

        /// <inheritdoc />
        public Length LeftTop { get; }

        /// <inheritdoc />
        public Length RightBottom { get; }

        /// <inheritdoc />
        public Length RightTop { get; }
    }
}