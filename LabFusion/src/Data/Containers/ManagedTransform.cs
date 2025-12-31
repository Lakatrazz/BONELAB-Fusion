using UnityEngine;

namespace LabFusion.Data;

public struct ManagedTransform
{
    public Vector3 Position;

    public Quaternion Rotation;

    public ManagedTransform(Vector3 position, Quaternion rotation)
    {
        Position = position;
        Rotation = rotation;
    }
}
