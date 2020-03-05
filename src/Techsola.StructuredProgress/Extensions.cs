using System.Collections.Generic;

namespace Techsola
{
    internal static class Extensions
    {
        public static IEnumerable<(int Index, T Value)> AsIndexed<T>(this IEnumerable<T> source)
        {
            var index = 0;

            foreach (var value in source)
            {
                yield return (index, value);
                index++;
            }
        }
    }
}
