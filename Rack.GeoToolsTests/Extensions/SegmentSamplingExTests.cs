using System;
using System.Collections.Generic;
using System.Linq;
using NetTopologySuite.Geometries;
using Rack.GeoTools.Extensions;
using Xunit;

namespace Rack.GeoToolsTests.Extensions
{
    public class SegmentSamplingExTests
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void GetSampledSegment_SegmentEndConsistentToStep_ReturnsWithEndIncludeArray(
            bool isEndInclude)
        {
            var startCoordinate = new Coordinate(0, 0);
            var endCoordinate = new Coordinate(5, 5);
            var step = startCoordinate.Distance(new Coordinate(1, 1));

            var expected = new[]
            {
                new Coordinate(0, 0),
                new Coordinate(1, 1),
                new Coordinate(2, 2),
                new Coordinate(3, 3),
                new Coordinate(4, 4),
                new Coordinate(5, 5),
            };
            var actual = startCoordinate.GetSampledSegment(endCoordinate, step, isEndInclude)
                .ToArray();

            Assert.Equal(expected, actual, new CoordinateComparer());
        }

        [Fact]
        public void GetSampledSegment_SegmentEndNotConsistentToStepAndWithEndInclude_ReturnsWithEndIncludeArray()
        {
            var startCoordinate = new Coordinate(0, 0);
            var endCoordinate = new Coordinate(5.5, 5.5);
            var step = startCoordinate.Distance(new Coordinate(1, 1));

            var expected = new[]
            {
                new Coordinate(0, 0),
                new Coordinate(1, 1),
                new Coordinate(2, 2),
                new Coordinate(3, 3),
                new Coordinate(4, 4),
                new Coordinate(5, 5),
                new Coordinate(5.5, 5.5)
            };
            var actual = startCoordinate.GetSampledSegment(endCoordinate, step)
                .ToArray();

            Assert.Equal(expected, actual, new CoordinateComparer());
        }

        [Fact]
        public void GetSampledSegment_SegmentEndNotConsistentToStepAndWithoutEndInclude_ReturnsWithoutEndIncludeArray()
        {
            var startCoordinate = new Coordinate(0, 0);
            var endCoordinate = new Coordinate(5.5, 5.5);
            var step = startCoordinate.Distance(new Coordinate(1, 1));

            var expected = new[]
            {
                new Coordinate(0, 0),
                new Coordinate(1, 1),
                new Coordinate(2, 2),
                new Coordinate(3, 3),
                new Coordinate(4, 4),
                new Coordinate(5, 5)
            };
            var actual = startCoordinate.GetSampledSegment(endCoordinate, step, false)
                .ToArray();

            Assert.Equal(1.0, 2.0, 3);

            Assert.Equal(expected, actual, new CoordinateComparer());
        }
    }

    class CoordinateComparer: EqualityComparer<Coordinate>
    {
        private readonly double _tolerance;

        public CoordinateComparer(): this(0.0001)
        { }

        public CoordinateComparer(double tolerance)
        {
            _tolerance = tolerance;
        }

        public override bool Equals(Coordinate x, Coordinate y)
        {
            if (x == y)
                return true;

            if (x == null || y == null)
                return false;

            return !(Math.Abs(x.X - y.X) > _tolerance) && 
                   !(Math.Abs(x.Y - y.Y) > _tolerance);
        }

        public override int GetHashCode(Coordinate obj)
        {
            throw new System.NotImplementedException();
        }
    }
}