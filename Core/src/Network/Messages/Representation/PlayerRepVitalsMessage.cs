using LabFusion.Data;
using LabFusion.Network;
using LabFusion.Representation;
using SLZ.Rig;
using SLZ.VRMK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LabFusion.Network
{
    public class SerializedBodyVitals : IFusionSerializable
    {
        public float height;
        public BodyVitals.MeasurementState measurement;
        public float chest;
        public float underbust;
        public float waist;
        public float hips;
        public float wingspan;
        public float inseam;
        public float sittingOffset;
        public float floorOffset;

        public SerializedBodyVitals() { }

        public SerializedBodyVitals(BodyVitals vitals)
        {
            height = vitals.realWorldHeight;
            measurement = vitals.measurementPresets;
            chest = vitals.chestCircumference;
            underbust = vitals.underbustCircumference;
            waist = vitals.waistCircumference;
            hips = vitals.hipsCircumference;
            wingspan = vitals.wingspan;
            sittingOffset = vitals.sittingOffset;
            floorOffset = vitals.floorOffset;
        }

        public void Serialize(FusionWriter writer)
        {
            writer.Write(height);
            writer.Write((byte)measurement);
            writer.Write(chest);
            writer.Write(underbust);
            writer.Write(waist);
            writer.Write(hips);
            writer.Write(wingspan);
            writer.Write(sittingOffset);
            writer.Write(floorOffset);
        }

        public void Deserialize(FusionReader reader)
        {
            height = reader.ReadSingle();
            measurement = (BodyVitals.MeasurementState)reader.ReadByte();
            chest = reader.ReadSingle();
            underbust = reader.ReadSingle();
            waist = reader.ReadSingle();
            hips = reader.ReadSingle();
            wingspan = reader.ReadSingle();
            sittingOffset = reader.ReadSingle();
            floorOffset = reader.ReadSingle();
        }

        public void CopyTo(BodyVitals vitals)
        {
            vitals.realWorldHeight = height;
            vitals.measurementPresets = measurement;
            vitals.chestCircumference = chest;
            vitals.underbustCircumference = underbust;
            vitals.waistCircumference = waist;
            vitals.hipsCircumference = hips;
            vitals.wingspan = wingspan;
            vitals.sittingOffset = sittingOffset;
            vitals.floorOffset = floorOffset;
        }
    }

    public class PlayerRepVitalsData : IFusionSerializable, IDisposable {
        public byte smallId;
        public SerializedBodyVitals bodyVitals;

        public void Serialize(FusionWriter writer) {
            writer.Write(smallId);
            writer.Write(bodyVitals);
        }

        public void Deserialize(FusionReader reader) {
            smallId = reader.ReadByte();
            bodyVitals = reader.ReadFusionSerializable<SerializedBodyVitals>();
        }

        public void Dispose() {
            GC.SuppressFinalize(this);
        }

        public static PlayerRepVitalsData Create(byte smallId, BodyVitals vitals) {
            return new PlayerRepVitalsData()
            {
                smallId = smallId,
                bodyVitals = new SerializedBodyVitals(vitals)
            };
        }
    }
    
    public class PlayerRepVitalsMessage : FusionMessageHandler
    {
        public override byte? Tag => NativeMessageTag.PlayerRepVitals;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false) {
            using (var reader = FusionReader.Create(bytes)) {
                var data = reader.ReadFusionSerializable<PlayerRepVitalsData>();

                if (PlayerRep.Representations.ContainsKey(data.smallId)) {
                    var rep = PlayerRep.Representations[data.smallId];
                    rep.SetVitals(data.bodyVitals);
                }

                if (NetworkUtilities.IsServer) {
                    using (var message = FusionMessage.Create(Tag.Value, bytes)) {
                        FusionMod.CurrentNetworkLayer.BroadcastMessageExcept(data.smallId, NetworkChannel.Reliable, message);
                    }
                }
            }
        }
    }
}
