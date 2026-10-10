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

    public readonly SkinnedMeshRenderer GetRenderer(Avatar avatar)
    {
        return MeshType switch
        {
            AvatarMeshType.Head => avatar.headMeshes[MeshIndex],
            AvatarMeshType.Hair => avatar.hairMeshes[MeshIndex],
            _ => avatar.bodyMeshes[MeshIndex],
        };
    }
}
