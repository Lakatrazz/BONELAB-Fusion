using LabFusion.Data;
using LabFusion.Representation;
using LabFusion.Utilities;
using SLZ.Rig;
using SLZ.VRMK;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.Network {
    public class PlayerRepTransformData : IFusionSerializable, IDisposable
    {
        public byte smallId;
        public float feetOffset;
        public SerializedTransform[] serializedTransforms = new SerializedTransform[PlayerRepUtilities.TransformSyncCount];
        public SerializedTransform serializedPelvis;
        public SerializedQuaternion serializedPlayspace;
        public Vector3 predictVelocity;

        public SerializedHand leftHand;
        public SerializedHand rightHand;


        public void Serialize(FusionWriter writer)
        {
            writer.Write(smallId);
            writer.Write(predictVelocity);
            writer.Write(feetOffset);

            for (var i = 0; i < PlayerRepUtilities.TransformSyncCount; i++)
                writer.Write(serializedTransforms[i]);

            writer.Write(serializedPelvis);
            writer.Write(serializedPlayspace);

            writer.Write(leftHand);
            writer.Write(rightHand);
        }

        public void Deserialize(FusionReader reader)
        {
            smallId = reader.ReadByte();
            predictVelocity = reader.ReadVector3();
            feetOffset = reader.ReadSingle();

            for (var i = 0; i < PlayerRepUtilities.TransformSyncCount; i++)
                serializedTransforms[i] = reader.ReadFusionSerializable<SerializedTransform>();

            serializedPelvis = reader.ReadFusionSerializable<SerializedTransform>();
            serializedPlayspace = reader.ReadFusionSerializable<SerializedQuaternion>();

            leftHand = reader.ReadFusionSerializable<SerializedHand>();
            rightHand = reader.ReadFusionSerializable<SerializedHand>();
        }

        public void Dispose() { 
            GC.SuppressFinalize(this);
        }

        public static PlayerRepTransformData Create(byte smallId, Transform[] syncTransforms, Transform syncedPelvis, Transform syncedPlayspace, BaseController leftHand, BaseController rightHand)
        {
            var data = new PlayerRepTransformData();
            data.smallId = smallId;
            data.predictVelocity = RigData.RigReferences.RigManager.physicsRig.torso._pelvisRb.velocity * Time.fixedDeltaTime;
            data.feetOffset = RigData.RigReferences.RigManager.openControllerRig.feetOffset;
            data.serializedPelvis = new SerializedTransform(syncedPelvis);
            data.serializedPlayspace = SerializedQuaternion.Compress(syncedPlayspace.rotation);

            data.leftHand = new SerializedHand(leftHand);
            data.rightHand = new SerializedHand(rightHand);

            for (var i = 0; i < PlayerRepUtilities.TransformSyncCount; i++) {
                data.serializedTransforms[i] = new SerializedTransform(syncTransforms[i].localPosition, syncTransforms[i].localRotation);
            }

            return data;
        }
    }

    public class PlayerRepTransformMessage : FusionMessageHandler {
        public override byte? Tag => NativeMessageTag.PlayerRepTransform;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false) {
            using (var reader = FusionReader.Create(bytes)) {
                var data = reader.ReadFusionSerializable<PlayerRepTransformData>();

                // Send message to other clients if server
                if (NetworkInfo.IsServer && isServerHandled) {
                    if (data.smallId != 0) {
                        using (var message = FusionMessage.Create(Tag.Value, bytes)) {
                            MessageSender.BroadcastMessageExcept(data.smallId, NetworkChannel.Unreliable, message);
                        }
                    }
                }

                // Apply player rep data
                if (data.smallId != PlayerIdManager.LocalSmallId && PlayerRep.Representations.ContainsKey(data.smallId)) {
                    var rep = PlayerRep.Representations[data.smallId];
                    rep.repControllerRig.feetOffset = data.feetOffset;
                    rep.serializedTransforms = data.serializedTransforms;
                    rep.serializedPelvis = data.serializedPelvis;
                    rep.repPlayspace.rotation = data.serializedPlayspace.Expand();
                    rep.predictVelocity = data.predictVelocity;
                    rep.timeSincePelvisSent = Time.realtimeSinceStartup;

                    data.leftHand.CopyTo(rep.repLeftController);
                    data.rightHand.CopyTo(rep.repRightController);
                }
            }
        }
    }
}
