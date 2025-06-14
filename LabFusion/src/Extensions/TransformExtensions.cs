using UnityEngine;

namespace LabFusion.Extensions;

public static class TransformExtensions
{
    internal static string GetBasePath(this Transform transform)
    {
        var name = transform.name;
        var parent = transform.parent;

        if (parent == null)
        {
            return $"{StringExtensions.UniqueSeparator}{name}";
        }

        return $"{parent.GetBasePath()}{StringExtensions.UniqueSeparator}{GetSiblingNameIndex(transform)}{StringExtensions.UniqueSeparator}{name}";
    }

    internal static List<Transform> FindSiblingsWithName(this Transform parent, string name)
    {
        var siblings = new List<Transform>(parent.childCount);

        for (var i = 0; i < parent.childCount; i++)
        {
            var child = parent.GetChild(i);

            if (child.name == name)
            {
                siblings.Add(child);
            }
        }

        return siblings;
    }

    internal static int GetSiblingNameIndex(this Transform transform)
    {
        var locals = FindSiblingsWithName(transform.parent, transform.name);
        return locals.FindIndex((t) => t == transform);
    }

    internal static Transform GetTransformByIndex(this Transform parent, int index, string name)
    {
        // Get matching siblings
        var matching = FindSiblingsWithName(parent, name);

        if (matching.Count <= 0)
        {
            return null;
        }

        // Check if we can actually grab a transform with the index
        if (matching.Count <= index || index < 0)
        {
            return matching[^1];
        }
        else
        {
            return matching[index];
        }
    }
}
