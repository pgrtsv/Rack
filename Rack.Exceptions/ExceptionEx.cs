using System;

namespace Rack.Exceptions
{
    public static class ExceptionEx
    {
        public static T WithAdditionalData<T>(
            this T exception, 
            string key, 
            object value)
            where T : Exception
        {
            exception.Data.Add(key, value);
            return exception;
        }
    }
}