using LabFusion.Network;

namespace LabFusion.Data
{
    public interface IFusionSerializable 
    {
        void Serialize(FusionWriter writer);

        void Deserialize(FusionReader reader);
    }
}
