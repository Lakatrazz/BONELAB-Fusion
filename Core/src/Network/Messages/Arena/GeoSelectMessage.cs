using LabFusion.Data;
using LabFusion.Patching;

namespace LabFusion.Network
{
    public class GeoSelectData : IFusionSerializable
    {
        public byte geoIndex;

        public void Serialize(FusionWriter writer)
        {
            writer.Write(geoIndex);
        }

        public void Deserialize(FusionReader reader)
        {
            geoIndex = reader.ReadByte();
        }

        public static GeoSelectData Create(byte geoIndex)
        {
            return new GeoSelectData()
            {
                geoIndex = geoIndex,
            };
        }
    }

    [Net.DelayWhileTargetLoading]
    public class GeoSelectMessage : FusionMessageHandler
    {
        public override byte? Tag => NativeMessageTag.GeoSelect;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
        {
            using FusionReader reader = FusionReader.Create(bytes);
            var data = reader.ReadFusionSerializable<GeoSelectData>();
            var manager = ArenaData.GeoManager;

            GeoManagerPatches.IgnorePatches = true;

            // We ONLY handle this for clients, this message should only ever be sent by the server!
            if (!NetworkInfo.IsServer && manager)
            {
                manager.ClearCurrentGeo();
                manager.ToggleGeo(data.geoIndex);
            }

            GeoManagerPatches.IgnorePatches = false;
        }
    }
}
