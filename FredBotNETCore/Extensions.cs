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
    }
}
