using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LabFusion.Data;
using LabFusion.Representation;
using LabFusion.Utilities;
using LabFusion.Grabbables;
using LabFusion.Syncables;
using LabFusion.Patching;

using SLZ;
using SLZ.Interaction;
using SLZ.Props.Weapons;

namespace LabFusion.Network
{
    public class GeoSelectData : IFusionSerializable, IDisposable
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

        public void Dispose()
        {
            GC.SuppressFinalize(this);
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
            using (FusionReader reader = FusionReader.Create(bytes))
            {
                using (var data = reader.ReadFusionSerializable<GeoSelectData>())
                {
                    var manager = ArenaData.GeoManager;

                    GeoManagerPatches.IgnorePatches = true;

                    // We ONLY handle this for clients, this message should only ever be sent by the server!
                    if (!NetworkInfo.IsServer && manager) {
                        manager.ClearCurrentGeo();
                        manager.ToggleGeo(data.geoIndex);
                    }

                    GeoManagerPatches.IgnorePatches = false;
                }
            }
        }
    }
}
