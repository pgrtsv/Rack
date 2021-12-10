using System.Collections.Generic;

namespace Rack.LocalizationTool.Infrastructure.Comparer
{
    /// <summary>
    /// Реализация <see cref="IComparer{T}"/> для сравнения namespace-ов.
    /// </summary>
    public class NamespacesComparer : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            var isFirstSystem = IsSystemNamespace(x);
            var isSecondSystem = IsSystemNamespace(y);
            if (isFirstSystem && isSecondSystem || !isFirstSystem && !isSecondSystem)
                return IsFirstMoreThan(x, y);
            return isFirstSystem ? -1 : 1;
        }

        private static bool IsSystemNamespace(string name)
        {
            return name?.Replace("using", "")
                       .Trim()
                       .Substring(0, 6) == "System";
        }

        private static int IsFirstMoreThan(string x, string y)
        {
            if (x == null && y == null)
                return 0;
            if (x == null) return -1;
            if (y == null) return 1;

            var minLength = x.Length < y.Length
                ? x.Length : y.Length;
            for (int i = 0; i < minLength; i++)
            {
                if (x[i] == y[i]) continue;
                if (x[i] == ';') return -1;
                if (y[i] == ';') return 1;
                if (x[i] > y[i])
                    return 1;
                return -1;
            }

            if (x.Length == y.Length) return 0;
            return x.Length < y.Length ? -1 : 1;
        }
    }
}
