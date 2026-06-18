using Il2CppSLZ.Marrow.Interaction;

using LabFusion.Data;
using LabFusion.Network.Serialization;

using UnityEngine;

namespace LabFusion.Marrow.Data;

public class MarrowEntityRepresentation : INetSerializable
{
    public static readonly MarrowEntityRepresentation Default = new();

    public SerializedBounds Bounds = SerializedBounds.Fallback;

    public SerializedTransform Offset = SerializedTransform.Default;

    public static MarrowEntityRepresentation CreateFromEntity(MarrowEntity marrowEntity)
    {
        if (marrowEntity.Bodies.Count <= 0)
        {
            return Default;
        }

        var rootBody = marrowEntity.Bodies[0];

        if (rootBody == null)
        {
            return Default;
        }

        SerializedBounds bounds;
        SerializedTransform offset = SerializedTransform.Default;

        var poolee = marrowEntity._poolee;

        if (poolee != null && poolee.SpawnableCrate != null)
        {
            var spawnableCrate = poolee.SpawnableCrate;

            bounds = new SerializedBounds(spawnableCrate.ColliderBounds);

            if (marrowEntity.gameObject != rootBody.gameObject)
            {
                var defaultRootBodyPose = marrowEntity._defaultPoseCache[0];

                offset = new SerializedTransform(-defaultRootBodyPose.position, Quaternion.Inverse(defaultRootBodyPose.rotation));
            }
        }
        else
        {
            bounds = new SerializedBounds(marrowEntity.Bodies[0].Bounds);
        }

        return new MarrowEntityRepresentation()
        {
            Bounds = bounds,
            Offset = offset,
        };
    }

    public int? GetSize() => Bounds.GetSize() + Offset.GetSize();

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref Bounds);
        serializer.SerializeValue(ref Offset);
    }
}
