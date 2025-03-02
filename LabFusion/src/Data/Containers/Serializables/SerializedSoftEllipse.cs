using Il2CppSLZ.VRMK;

using LabFusion.Network.Serialization;

namespace LabFusion.Data;

public class SerializedSoftEllipse : INetSerializable
{
    public const int Size = sizeof(float) * 4;

    public Avatar.SoftEllipse ellipse;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref ellipse.XRadius);
        serializer.SerializeValue(ref ellipse.XBias);
        serializer.SerializeValue(ref ellipse.ZRadius);
        serializer.SerializeValue(ref ellipse.ZBias);
    }

    public static implicit operator Avatar.SoftEllipse(SerializedSoftEllipse ellipse)
    {
        return ellipse.ellipse;
    }

    public static implicit operator SerializedSoftEllipse(Avatar.SoftEllipse ellipse)
    {
        return new SerializedSoftEllipse()
        {
            ellipse = ellipse,
        };
    }
}
