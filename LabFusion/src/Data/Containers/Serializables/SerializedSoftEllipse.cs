using LabFusion.Network;

using Il2CppSLZ.VRMK;

namespace LabFusion.Data
{
    public class SerializedSoftEllipse : IFusionSerializable
    {
        public const int Size = sizeof(float) * 4;

        public Avatar.SoftEllipse ellipse;

        public void Serialize(FusionWriter writer)
        {
            writer.Write(ellipse.XRadius);
            writer.Write(ellipse.XBias);
            writer.Write(ellipse.ZRadius);
            writer.Write(ellipse.ZBias);
        }

        public void Deserialize(FusionReader reader)
        {
            ellipse = new Avatar.SoftEllipse(
                reader.ReadSingle(),
                reader.ReadSingle(),
                reader.ReadSingle(),
                reader.ReadSingle()
                );
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
}
