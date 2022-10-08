using LabFusion.Data;
using LabFusion.Representation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LabFusion.Network {
    public class PlayerRepTransformData : IFusionSerializable, IDisposable
    {
        public const int TransformCount = 3;

        public byte smallId;
        public SerializedTransform[] serializedTransforms = new SerializedTransform[TransformCount];
        public Vector3 rootPosition;


        public void Serialize(FusionWriter writer)
        {
            writer.Write(smallId);

            for (var i = 0; i < TransformCount; i++)
                writer.Write(serializedTransforms[i]);

            writer.Write(rootPosition);
        }

        public void Deserialize(FusionReader reader)
        {
            smallId = reader.ReadByte();

            for (var i = 0; i < TransformCount; i++)
                serializedTransforms[i] = reader.ReadFusionSerializable<SerializedTransform>();

            rootPosition = reader.ReadVector3();
        }

        public void Dispose() { 
            GC.SuppressFinalize(this);
        }

        public static PlayerRepTransformData Create(byte smallId, Transform[] syncTransforms)
        {
            var data = new PlayerRepTransformData();
            data.smallId = smallId;
            data.rootPosition = Vector3.zero;

            for (var i = 0; i < TransformCount; i++) {
                data.serializedTransforms[i] = new SerializedTransform(syncTransforms[i]);
            }

            return data;
        }
    }

    public class PlayerRepTransformMessage : FusionMessageHandler {
        public override byte? Tag => NativeMessageTag.PlayerRepTransform;

        public override void HandleMessage(byte[] bytes) {
            using (var reader = FusionReader.Create(bytes)) {
                var data = reader.ReadFusionSerializable<PlayerRepTransformData>();
                
                if (PlayerRep.Representations.ContainsKey(data.smallId)) {
                    var rep = PlayerRep.Representations[data.smallId];
                    for (var i = 0; i < PlayerRepTransformData.TransformCount; i++) {
                        rep.repTransforms[i].position = data.serializedTransforms[i].position;
                        rep.repTransforms[i].rotation = data.serializedTransforms[i].rotation.Expand();
                    }
                }
            }
        }
    }
}
