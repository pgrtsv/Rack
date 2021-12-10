using System;

namespace Rack.GeoTools
{
    public enum CreationExceptionKind
    {
        XAxisLimitsInverted,
        YAxisLimitsInverted,
        CountUnequality
    }

    public sealed class SurfaceCreationException : Exception
    {
        public CreationExceptionKind Kind { get; }

        public SurfaceCreationException(CreationExceptionKind kind) => Kind = kind;

        public SurfaceCreationException(CreationExceptionKind kind, Exception innerException)
            : base(string.Empty, innerException)
            => Kind = kind;
    }

    public enum ReadFileExceptionKind
    {
        FileNotExists,
        UnableToReadFile
    }

    public sealed class SurfaceReadFileException : Exception
    {
        public ReadFileExceptionKind Kind { get; }

        public SurfaceReadFileException(ReadFileExceptionKind kind) => Kind = kind;

        public SurfaceReadFileException(ReadFileExceptionKind kind, Exception innerException)
            : base(string.Empty, innerException)
            => Kind = kind;
    }

    public enum GetZExceptionKind
    {
        XOutOfBounds,
        YOutOfBounds,
        UnknownDerivative
    }

    public class SurfaceGetZException : Exception
    {
        public GetZExceptionKind Kind { get; }

        public SurfaceGetZException(GetZExceptionKind kind) => Kind = kind;

        public SurfaceGetZException(GetZExceptionKind kind, Exception innerException)
            : base(string.Empty, innerException)
            => Kind = kind;
    }
}