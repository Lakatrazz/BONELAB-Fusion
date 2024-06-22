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

    // .NET Core's string hashing isn't deterministic on game restart, only during the same instance.
    // Credits to https://andrewlock.net/why-is-string-gethashcode-different-each-time-i-run-my-program-in-net-core/#a-deterministic-gethashcode-implementation
    // In this case, I reaally need it to be deterministic, so
    private static int GetDeterministicHashCode(this string str)
    {
        unchecked
        {
            int hash1 = (5381 << 16) + 5381;
            int hash2 = hash1;

            for (int i = 0; i < str.Length; i += 2)
            {
                hash1 = ((hash1 << 5) + hash1) ^ str[i];
                if (i == str.Length - 1)
                    break;
                hash2 = ((hash2 << 5) + hash2) ^ str[i + 1];
            }

            return hash1 + (hash2 * 1566083941);
        }
    }
}
