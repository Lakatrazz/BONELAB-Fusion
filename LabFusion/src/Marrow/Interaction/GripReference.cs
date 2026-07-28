using Il2CppSLZ.Marrow;

using LabFusion.Entities;
using LabFusion.Marrow.Extenders;
using LabFusion.Network;
using LabFusion.Network.Serialization;
using LabFusion.Patching;

namespace LabFusion.Marrow.Interaction;

public enum GripType
{
    None,

    Path,

    World,
}

public struct GripReference : INetSerializable
{
    public static readonly GripReference None = new() { Type = GripType.None };

    public GripType Type;

    public ComponentPathData Path;

    public NetworkEntityReference WorldRigReference;

    public static GripReference CreateFromGrip(Grip grip)
    {
        var worldGrip = grip.TryCast<WorldGrip>();

        if (worldGrip != null)
        {
            return CreateFromWorldGrip(worldGrip);
        }

        var pathData = ComponentPathData.CreateFromComponent<Grip, GripExtender>(grip, GripPatches.HashTable, GripExtender.Cache);

        if (pathData != null)
        {
            return new GripReference()
            {
                Type = GripType.Path,
                Path = pathData,
            };
        }

        return None;
    }

    public readonly bool TryGetGrip(out Grip grip)
    {
        grip = null;

        switch (Type)
        {
            case GripType.Path:
                if (Path.TryGetComponent<Grip, GripExtender>(GripPatches.HashTable, out grip))
                {
                    return true;
                }

                break;
            case GripType.World:
                if (!WorldRigReference.TryGetEntity(out var worldRigEntity))
                {
                    return false;
                }

                var worldRig = worldRigEntity.GetExtender<NetworkRig>();

                if (worldRig == null || !worldRig.HasRig)
                {
                    return false;
                }

                grip = worldRig.RigRefs.RigManager.worldGrip;
                return true;
        }

        return false;
    }

    public readonly int? GetSize()
    {
        int? size = sizeof(byte);

        switch (Type)
        {
            case GripType.Path:
                size += Path.GetSize();
                break;
            case GripType.World:
                size += NetworkEntityReference.Size;
                break;
        }

        return size;
    }

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref Type);

        switch (Type)
        {
            case GripType.Path:
                serializer.SerializeValue(ref Path);
                break;
            case GripType.World:
                serializer.SerializeValue(ref WorldRigReference);
                break;
        }
    }

    private static GripReference CreateFromWorldGrip(WorldGrip worldGrip)
    {
        if (!NetworkRig.WorldGripCache.TryGet(worldGrip, out var worldGripRig))
        {
            return None;
        }

        return new GripReference()
        {
            Type = GripType.World,
            WorldRigReference = new(worldGripRig.NetworkEntity),
        };
    }
}
