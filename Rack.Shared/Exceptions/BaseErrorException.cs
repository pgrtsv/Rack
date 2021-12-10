using System;

namespace Rack.Shared.Exceptions
{
    [Obsolete("Использование этого абстрактного класса не имеет смысла.")]
    public abstract class BaseErrorException<T> : Exception where T : Enum
    {
        public BaseErrorException(T kind)
        {
            Kind = kind;
        }

        public BaseErrorException(T kind, Exception innerException) : base(string.Empty, innerException)
        {
            Kind = kind;
        }

        public T Kind { get; }
    }
}