using System;

namespace Rack.Shared
{
    public static class UriExtensions
    {
        [Obsolete]
        public static Uri GetRelativeUri<T>()
        {
            return new Uri(typeof(T).Name, UriKind.Relative);
        }
    }
}