using System;
using System.Collections.Generic;
using System.Linq;

namespace FredBotNETCore
{
    public static class Extensions
    {
        private static Random random = new Random();

        public static T GetRandomElement<T>(this IEnumerable<T> list)
        {
            if (list.Count() == 0)
                return default(T);

            return list.ElementAt(random.Next(list.Count()));
        }

        public static IEnumerable<String> SplitInParts(this String s, Int32 partLength)
        {
            if (s == null)
                throw new ArgumentNullException("s");
            if (partLength <= 0)
                throw new ArgumentException("Part length has to be positive.", "partLength");

            for (var i = 0; i < s.Length; i += partLength)
                yield return s.Substring(i, Math.Min(partLength, s.Length - i));
        }

        public static bool Contains(this string source, string toCheck, StringComparison comp)
        {
            return source?.IndexOf(toCheck, comp) >= 0;
        }
    }
}
