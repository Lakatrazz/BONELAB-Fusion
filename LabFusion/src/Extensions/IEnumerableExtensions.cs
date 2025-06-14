using LabFusion.Utilities;

namespace LabFusion.Extensions;

public static class IEnumerableExtensions
{
    public static T GetRandom<T>(this IEnumerable<T> enumerable)
    {
        int random = UnityEngine.Random.Range(0, enumerable.Count());
        return enumerable.ElementAt(random);
    }

    public static bool ContainsInstance<T>(this List<T> list, T obj) where T : class
    {
        return list.Any((o) => o == obj);
    }

    public static bool RemoveInstance<T>(this List<T> list, T obj) where T : class
    {
        if (!list.ContainsInstance(obj))
            return false;

        for (var i = 0; i < list.Count; i++)
        {
            if (list.ElementAt(i) == obj)
            {
                list.RemoveAt(i);
                return true;
            }
        }

        return false;
    }

    public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
    {
        for (var i = 0; i < enumerable.Count(); i++)
        {
            action(enumerable.ElementAt(i));
        }
    }

    public static bool Has<T>(this IEnumerable<T> list, T obj) where T : UnityEngine.Object => list.Any(o => o == obj);

    public static bool Has<T>(this Il2CppSystem.Collections.Generic.List<T> list, T obj) where T : UnityEngine.Object
    {
        for (var i = 0; i < list.Count; i++)
        {
            var other = list[i];

            if (other == obj)
                return true;
        }

        return false;
    }

    private static readonly Random _random = new();

    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = _random.Next(n + 1);

            // Swap values
            (list[n], list[k]) = (list[k], list[n]);
        }
    }
}