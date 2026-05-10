using UnityEngine;

namespace LabFusion.Utilities;

/// <summary>
/// Temporarily unparents transforms and stores their original parent so that they can be reparented.
/// </summary>
public sealed class TemporaryTransformDetacher
{
    public Dictionary<Transform, Transform> TransformToParentLookup { get; } = new();
    
    public void DetachTransform(Transform transform)
    {
        var parent = transform.parent;

        TransformToParentLookup[transform] = parent;

        transform.parent = null;
    }

    public void ReattachTransforms()
    {
        foreach (var pair in TransformToParentLookup)
        {
            pair.Key.parent = pair.Value;
        }

        TransformToParentLookup.Clear();
    }
}
