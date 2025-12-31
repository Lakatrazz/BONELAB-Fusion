using Il2CppSLZ.VRMK;

using LabFusion.Network.Serialization;

namespace LabFusion.Marrow.Serialization;

public struct SerializedSoftEllipse : INetSerializable
{
    public const int Size = sizeof(float) * 4;

    public Avatar.SoftEllipse Ellipse;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref Ellipse.XRadius);
        serializer.SerializeValue(ref Ellipse.XBias);
        serializer.SerializeValue(ref Ellipse.ZRadius);
        serializer.SerializeValue(ref Ellipse.ZBias);
    }

    public static implicit operator Avatar.SoftEllipse(SerializedSoftEllipse ellipse)
    {
        return ellipse.Ellipse;
    }

    public static implicit operator SerializedSoftEllipse(Avatar.SoftEllipse ellipse)
    {
        return new SerializedSoftEllipse()
        {
            Ellipse = ellipse,
        };
    }
}
