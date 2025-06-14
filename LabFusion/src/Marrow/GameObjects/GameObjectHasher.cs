using LabFusion.Extensions;

using System.Text;

using UnityEngine;

namespace LabFusion.Marrow;

public static class GameObjectHasher
{
    public static int GetFastHash(GameObject gameObject)
    {
        Transform transform = gameObject.transform;
        StringBuilder hashBuilder = new();

        // Add start name and sibling index
        hashBuilder.Append(transform.name);
        hashBuilder.Append(transform.GetSiblingIndex());

        // Add closest parent name
        if (transform.parent != null)
        {
            hashBuilder.Append(transform.parent.name);
        }

        string hashString = hashBuilder.ToString();

        return hashString.GetDeterministicHashCode();
    }

    public static int GetHierarchyHash(GameObject gameObject)
    {
        Transform transform = gameObject.transform;
        StringBuilder hashBuilder = new();

        // Add start name and sibling index
        hashBuilder.Append(transform.name);
        hashBuilder.Append(transform.GetSiblingIndex());

        // Add scene name
        hashBuilder.Append(gameObject.scene.name);

        // Add parent names
        var parent = transform.parent;

        // Accidentally made this an infinite loop once, make sure the next parent is always changing
        while (parent != null)
        {
            hashBuilder.Append(parent.name);
            parent = parent.parent;
        }

        string hashString = hashBuilder.ToString();

        return hashString.GetDeterministicHashCode();
    }
}
