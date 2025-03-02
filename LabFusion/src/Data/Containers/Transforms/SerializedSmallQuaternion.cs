using UnityEngine;

using LabFusion.Network;
using LabFusion.Extensions;
using LabFusion.Network.Serialization;

namespace LabFusion.Data;

public class SerializedSmallQuaternion : INetSerializable
{
    public const int Size = sizeof(byte) * 4;
    public static readonly SerializedSmallQuaternion Default = Compress(QuaternionExtensions.identity);

    public sbyte c1, c2, c3, c4;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref c1);
        serializer.SerializeValue(ref c2);
        serializer.SerializeValue(ref c3);
        serializer.SerializeValue(ref c4);
    }

    public static SerializedSmallQuaternion Compress(Quaternion quat)
    {
        return new SerializedSmallQuaternion() 
        {
            c1 = quat.x.ToSByte(),
            c2 = quat.y.ToSByte(),
            c3 = quat.z.ToSByte(),
            c4 = quat.w.ToSByte()
        };
    }

    public Quaternion Expand()
    {
        return new Quaternion(c1.ToSingle(), c2.ToSingle(), c3.ToSingle(), c4.ToSingle()).normalized;
    }
}