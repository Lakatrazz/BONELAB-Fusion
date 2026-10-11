using Il2CppInterop.Runtime.InteropTypes.Arrays;

using LabFusion.Marrow.Rig;
using LabFusion.Network.Serialization;

using UnityEngine;

using Avatar = Il2CppSLZ.VRMK.Avatar;

namespace LabFusion.Marrow.Data;

public struct AvatarBlendShapePath : INetSerializable
{
    public const int Size = sizeof(byte) + sizeof(int) * 2 + sizeof(float);

    public AvatarMeshType MeshType;

    public int MeshIndex;

    public int BlendShapeIndex;

    public float BlendShapeValue;

    public readonly int? GetSize() => Size;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref MeshType, Precision.OneByte);
        serializer.SerializeValue(ref MeshIndex);
        serializer.SerializeValue(ref BlendShapeIndex);
        serializer.SerializeValue(ref BlendShapeValue);
    }

    public readonly bool TryGetRenderer(Avatar avatar, out SkinnedMeshRenderer renderer)
    {
        renderer = null;

        var renderers = GetRenderers(avatar);

        if (renderers == null)
        {
            return false;
        }

        if (renderers.Length <= MeshIndex)
        {
            return false;
        }

        renderer = renderers[MeshIndex];
        return renderer != null;
    }

    public readonly Il2CppReferenceArray<SkinnedMeshRenderer> GetRenderers(Avatar avatar)
    {
        return MeshType switch
        {
            AvatarMeshType.Head => avatar.headMeshes,
            AvatarMeshType.Hair => avatar.hairMeshes,
            _ => avatar.bodyMeshes,
        };
    }
}
