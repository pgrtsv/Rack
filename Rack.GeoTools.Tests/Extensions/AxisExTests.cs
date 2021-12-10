using Rack.GeoTools.Extensions;
using Xunit;

namespace Rack.GeoTools.Tests.Extensions
{
    public class AxisExTests
    {
        [Fact]
        public void GetStepConsistency_DoubleImpl_Step50Value200ModeMoreThanSource_Expected200()
        {
            const double value = 200;
            const int step = 50;
            const StepConsistencyMode mode = StepConsistencyMode.MoreThanSource;

            const int expected = 200;
            var actual = value.GetStepConsistency(step, mode, 1);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetStepConsistency_DoubleImpl_Step50Value200ModeLessThanSource_Expected200()
        {
            const double value = 200;
            const int step = 50;
            const StepConsistencyMode mode = StepConsistencyMode.LessThanSource;

            const int expected = 200;
            var actual = value.GetStepConsistency(step, mode, 1);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetStepConsistency_DoubleImpl_Step50Value199ModeMoreThanSource_Expected200()
        {
            const double value = 199;
            const int step = 50;
            const StepConsistencyMode mode = StepConsistencyMode.MoreThanSource;

            const int expected = 200;
            var actual = value.GetStepConsistency(step, mode, 1);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetStepConsistency_DoubleImpl_Step50Value199ModeLessThanSource_Expected150()
        {
            const double value = 199;
            const int step = 50;
            const StepConsistencyMode mode = StepConsistencyMode.LessThanSource;

            const int expected = 150;
            var actual = value.GetStepConsistency(step, mode, 1);

            Assert.Equal(expected, actual);
        }
        
        [Fact]
        public void GetStepConsistency_DoubleImpl_Step005ValueNegative0850ModeMoreThanSource_ExpectedNegative0850()
        {
            const double value = -0.85;
            const double step = 0.05;
            const StepConsistencyMode mode = StepConsistencyMode.MoreThanSource;

            const double expected = -0.85;
            var actual = value.GetStepConsistency(step, mode, 3);

            Assert.Equal(expected, actual, 3);
        }

        [Fact]
        public void GetStepConsistency_DoubleImpl_Step005ValueNegative0850ModeLessThanSource_ExpectedNegative0850()
        {
            const double value = -0.85;
            const double step = 0.05;
            const StepConsistencyMode mode = StepConsistencyMode.LessThanSource;

            const double expected = -0.85;
            var actual = value.GetStepConsistency(step, mode, 3);

            Assert.Equal(expected, actual, 3);
        }

        [Fact]
        public void GetStepConsistency_DoubleImpl_Step005ValueNegative0874ModeMoreThanSource_ExpectedNegative0850()
        {
            const double value = -0.874;
            const double step = 0.05;
            const StepConsistencyMode mode = StepConsistencyMode.MoreThanSource;

            const double expected = -0.85;
            var actual = value.GetStepConsistency(step, mode, 3);

            Assert.Equal(expected, actual, 3);
        }

        [Fact]
        public void GetStepConsistency_DoubleImpl_Step005ValueNegative0874ModeLessThanSource_ExpectedNegative0900()
        {
            const double value = -0.874;
            const double step = 0.05;
            const StepConsistencyMode mode = StepConsistencyMode.LessThanSource;

            const double expected = -0.9;
            var actual = value.GetStepConsistency(step, mode, 3);

            Assert.Equal(expected, actual, 3);
        }

        [Fact]
        public void GetStepConsistency_IntImpl_Step5Value15ModeMoreThanSource_Expected15()
        {
            const int value = 15;
            const int step = 5;
            const StepConsistencyMode mode = StepConsistencyMode.MoreThanSource;

            const int expected = 15;
            var actual = value.GetStepConsistency(step, mode);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetStepConsistency_IntImpl_Step5Value15ModeLessThanSource_Expected15()
        {
            const int value = 15;
            const int step = 5;
            const StepConsistencyMode mode = StepConsistencyMode.LessThanSource;

            const int expected = 15;
            var actual = value.GetStepConsistency(step, mode);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetStepConsistency_IntImpl_Step5Value13ModeMoreThanSource_Expected15()
        {
            const int value = 13;
            const int step = 5;
            const StepConsistencyMode mode = StepConsistencyMode.MoreThanSource;

            const int expected = 15;
            var actual = value.GetStepConsistency(step, mode);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetStepConsistency_IntImpl_Step5Value13ModeLessThanSource_Expected10()
        {
            const int value = 13;
            const int step = 5;
            const StepConsistencyMode mode = StepConsistencyMode.LessThanSource;

            const int expected = 10;
            var actual = value.GetStepConsistency(step, mode);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetStepConsistency_IntImpl_Step5ValueNegative13ModeMoreThanSource_ExpectedNegative10()
        {
            const int value = -13;
            const int step = 5;
            const StepConsistencyMode mode = StepConsistencyMode.MoreThanSource;

            const int expected = -10;
            var actual = value.GetStepConsistency(step, mode);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetStepConsistency_IntImpl_Step5ValueNegative13ModeLessThanSource_ExpectedNegative15()
        {
            const int value = -13;
            const int step = 5;
            const StepConsistencyMode mode = StepConsistencyMode.LessThanSource;

            const int expected = -15;
            var actual = value.GetStepConsistency(step, mode);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetStepConsistency_IntImpl_Step5Value1ModeMoreThanSource_Expected5()
        {
            const int value = 1;
            const int step = 5;
            const StepConsistencyMode mode = StepConsistencyMode.MoreThanSource;

            const int expected = 5;
            var actual = value.GetStepConsistency(step, mode);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetStepConsistency_IntImpl_Step5Value1ModeLessThanSource_Expected0()
        {
            const int value = 1;
            const int step = 5;
            const StepConsistencyMode mode = StepConsistencyMode.LessThanSource;

            const int expected = 0;
            var actual = value.GetStepConsistency(step, mode);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetStepConsistency_IntImpl_Step5ValueNegative1ModeMoreThanSource_Expected0()
        {
            const int value = -1;
            const int step = 5;
            const StepConsistencyMode mode = StepConsistencyMode.MoreThanSource;

            const int expected = 0;
            var actual = value.GetStepConsistency(step, mode);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetStepConsistency_IntImpl_Step5ValueNegative1ModeLessThanSource_ExpectedNegative5()
        {
            const int value = -1;
            const int step = 5;
            const StepConsistencyMode mode = StepConsistencyMode.LessThanSource;

            const int expected = -5;
            var actual = value.GetStepConsistency(step, mode);

            Assert.Equal(expected, actual);
        }
    }
}