using LabFusion.Data;
using LabFusion.Representation;
using SLZ.Rig;
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
        public SerializedTransform serializedPelvis;
        public SerializedQuaternion serializedControllerRig;
        public SerializedQuaternion serializedPlayspace;

        public SerializedHand leftHand;
        public SerializedHand rightHand;


        public void Serialize(FusionWriter writer)
        {
            writer.Write(smallId);

            for (var i = 0; i < TransformCount; i++)
                writer.Write(serializedTransforms[i]);

            writer.Write(serializedPelvis);
            writer.Write(serializedControllerRig);
            writer.Write(serializedPlayspace);

            writer.Write(leftHand);
            writer.Write(rightHand);
        }

        public void Deserialize(FusionReader reader)
        {
            smallId = reader.ReadByte();

            for (var i = 0; i < TransformCount; i++)
                serializedTransforms[i] = reader.ReadFusionSerializable<SerializedTransform>();

            serializedPelvis = reader.ReadFusionSerializable<SerializedTransform>();
            serializedControllerRig = reader.ReadFusionSerializable<SerializedQuaternion>();
            serializedPlayspace = reader.ReadFusionSerializable<SerializedQuaternion>();

            leftHand = reader.ReadFusionSerializable<SerializedHand>();
            rightHand = reader.ReadFusionSerializable<SerializedHand>();
        }

        public void Dispose() { 
            GC.SuppressFinalize(this);
        }

        public static PlayerRepTransformData Create(byte smallId, Transform[] syncTransforms, Transform syncedPelvis, Transform syncedControllerRig, Transform syncedPlayspace, BaseController leftHand, BaseController rightHand)
        {
            var data = new PlayerRepTransformData();
            data.smallId = smallId;
            data.serializedPelvis = new SerializedTransform(syncedPelvis);
            data.serializedControllerRig = SerializedQuaternion.Compress(syncedControllerRig.rotation);
            data.serializedPlayspace = SerializedQuaternion.Compress(syncedPlayspace.rotation);
            data.leftHand = new SerializedHand(leftHand);
            data.rightHand = new SerializedHand(rightHand);

            for (var i = 0; i < TransformCount; i++) {
                data.serializedTransforms[i] = new SerializedTransform(syncTransforms[i].localPosition, syncTransforms[i].localRotation);
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
                    rep.serializedTransforms = data.serializedTransforms;
                    rep.serializedPelvisPos = data.serializedPelvis.position;
                    rep.repControllerRig.transform.rotation = data.serializedControllerRig.Expand();
                    rep.repControllerRig.vrRoot.rotation = data.serializedPlayspace.Expand();

                    data.leftHand.CopyTo(rep.repLeftController);
                    data.rightHand.CopyTo(rep.repRightController);
                }

                if (NetworkUtilities.IsServer) {
                    using (var message = FusionMessage.Create(Tag.Value, bytes)) {
                        FusionMod.CurrentNetworkLayer.BroadcastMessageExcept(data.smallId, NetworkChannel.Unreliable, message);
                    }
                }
            }
        }
    }
}
