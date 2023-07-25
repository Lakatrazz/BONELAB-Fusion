using BoneLib;
using System.Collections.Generic;
using Object = UnityEngine.Object;

namespace LabFusion.Extensions
{
    public class UnityComparer : IEqualityComparer<Object>
    {
        public bool Equals(Object lft, Object rht) {
            //MelonLoader.MelonLogger.Msg("comparing " + lft?.name ?? "NULL" + " with " + rht?.name ?? "NULL");
            return (lft?.GetInstanceID() ?? 0) == (rht?.GetInstanceID() ?? 0);
        }

        public int GetHashCode(Object obj) { 
            return obj.GetHashCode();
        }
    }

    public static class EnumerableExtensions
    {
        public static bool TryGetValueUnity<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, out TValue value) where TKey : UnityEngine.Object where TValue : class
        {
            if (!HelperMethods.IsAndroid())
            {
                if (dict.TryGetValue(key, out value))
                    return true;
            }
            else
            {
                foreach (var a in dict)
                {
                    if (a.Key.GetInstanceID() == key.GetInstanceID())
                    {
                        value = a.Value;
                        return true;
                    }
                }
            }

            value = default;
            return false;
        }

        public static bool TryGetValueC<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, out TValue value)
        {
            if (!HelperMethods.IsAndroid())
            {
                if (dict.TryGetValue(key, out value))
                    return true;
            }
            else
            {
                foreach (var kvp in dict)
                {
                    if (dict.Comparer.Equals(kvp.Key, key))
                    {
                        value = kvp.Value;
                        return true;
                    }
                }
            }

            value = default;
            return false;
        }
    }
}

