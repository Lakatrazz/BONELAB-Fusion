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
        public const int Size = sizeof(float) * 9 + sizeof(byte) * 4 + sizeof(int) * 2;

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
        public bool bodyLogFlipped;
        public bool bodyLogEnabled;

        public bool isRightHanded;

        public int loco_CurveMode;
        public int loco_Direction;

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

            bodyLogFlipped = vitals.bodyLogFlipped;
            bodyLogEnabled = vitals.bodyLogEnabled;

            isRightHanded = vitals.isRightHanded;

            loco_CurveMode = vitals.loco_CurveMode;
            loco_Direction = vitals.loco_Direction;
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

            writer.Write(bodyLogFlipped);
            writer.Write(bodyLogEnabled);

            writer.Write(isRightHanded);

            writer.Write(loco_CurveMode);
            writer.Write(loco_Direction);
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

            bodyLogFlipped = reader.ReadBoolean();
            bodyLogEnabled = reader.ReadBoolean();

            isRightHanded = reader.ReadBoolean();

            loco_CurveMode = reader.ReadInt32();
            loco_Direction = reader.ReadInt32();
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

            vitals.bodyLogFlipped = bodyLogFlipped;
            vitals.bodyLogEnabled = bodyLogEnabled;

            vitals.isRightHanded = isRightHanded;

            vitals.loco_CurveMode = loco_CurveMode;
            vitals.loco_Direction = loco_Direction;

            vitals.PROPEGATE();
            vitals.CalibratePlayerBodyScale();
        }
    }

    public class PlayerRepVitalsData : IFusionSerializable, IDisposable {
        public const int Size = SerializedBodyVitals.Size + sizeof(byte);

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

                if (PlayerRepManager.TryGetPlayerRep(data.smallId, out var rep)) {
                    rep.SetVitals(data.bodyVitals);
                }

                if (NetworkInfo.IsServer) {
                    using (var message = FusionMessage.Create(Tag.Value, bytes)) {
                        MessageSender.BroadcastMessageExcept(data.smallId, NetworkChannel.Reliable, message);
                    }
                }
            }
        }
    }
}
