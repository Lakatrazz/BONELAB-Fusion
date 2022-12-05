using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Extensions {
    public static class IEnumerableExtensions {
        public static bool ContainsInstance<T>(this List<T> list, T obj) where T : class {
            return list.Any((o) => o == obj);
        }

        public static bool RemoveInstance<T>(this List<T> list, T obj) where T : class {
            if (!list.ContainsInstance(obj))
                return false;

            for (var i = 0; i < list.Count(); i++) {
                if (list.ElementAt(i) == obj) {
                    list.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }
        
        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            foreach (T iterator in enumerable)
                action(iterator);
        }

        public static bool Has<T>(this IEnumerable<T> list, T obj) where T : UnityEngine.Object => list.Any(o => o == obj);
    }
}
