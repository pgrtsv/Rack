using System;
using System.Runtime.Serialization;

namespace Rack.Localization
{
    public class LocalizationServiceException : Exception
    {
        public LocalizationServiceException()
        {
        }

        public LocalizationServiceException(string message) : base(message)
        {
        }

        public LocalizationServiceException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected LocalizationServiceException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}