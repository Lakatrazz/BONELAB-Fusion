using LabFusion.Data;
using LabFusion.Extensions;
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
        public SerializedLocalTransform[] serializedLocalTransforms = new SerializedLocalTransform[PlayerRepUtilities.TransformSyncCount];
        public SerializedTransform serializedPelvis;
        public SerializedTransform serializedFootball;
        public SerializedSmallQuaternion serializedPlayspace;
        public SerializedSmallVector3 predictVelocity;

        public SerializedHand leftHand;
        public SerializedHand rightHand;


        public void Serialize(FusionWriter writer)
        {
            writer.Write(smallId);
            writer.Write(predictVelocity);
            writer.Write(feetOffset);

            for (var i = 0; i < PlayerRepUtilities.TransformSyncCount; i++)
                writer.Write(serializedLocalTransforms[i]);

            writer.Write(serializedPelvis);
            writer.Write(serializedFootball);
            writer.Write(serializedPlayspace);

            writer.Write(leftHand);
            writer.Write(rightHand);
        }

        public void Deserialize(FusionReader reader)
        {
            smallId = reader.ReadByte();
            predictVelocity = reader.ReadFusionSerializable<SerializedSmallVector3>();
            feetOffset = reader.ReadSingle();

            for (var i = 0; i < PlayerRepUtilities.TransformSyncCount; i++)
                serializedLocalTransforms[i] = reader.ReadFusionSerializable<SerializedLocalTransform>();

            serializedPelvis = reader.ReadFusionSerializable<SerializedTransform>();
            serializedFootball = reader.ReadFusionSerializable<SerializedTransform>();
            serializedPlayspace = reader.ReadFusionSerializable<SerializedSmallQuaternion>();

            leftHand = reader.ReadFusionSerializable<SerializedHand>();
            rightHand = reader.ReadFusionSerializable<SerializedHand>();
        }

        public void Dispose() { 
            GC.SuppressFinalize(this);
        }

        public static PlayerRepTransformData Create(byte smallId, Transform[] syncTransforms, Transform syncedPelvis, Transform syncedFootball, Transform syncedPlayspace, BaseController leftHand, BaseController rightHand)
        {
            var data = new PlayerRepTransformData {
                smallId = smallId,
                predictVelocity = SerializedSmallVector3.Compress(RigData.RigReferences.RigManager.physicsRig.torso._pelvisRb.velocity * Time.timeScale),
                feetOffset = RigData.RigReferences.RigManager.openControllerRig.feetOffset,
                serializedPelvis = new SerializedTransform(syncedPelvis),
                serializedFootball = new SerializedTransform(syncedFootball),

                serializedPlayspace = SerializedSmallQuaternion.Compress(syncedPlayspace.rotation),

                leftHand = new SerializedHand(leftHand),
                rightHand = new SerializedHand(rightHand)
            };

            for (var i = 0; i < PlayerRepUtilities.TransformSyncCount; i++) {
                data.serializedLocalTransforms[i] = new SerializedLocalTransform(syncTransforms[i]);
            }

            return data;
        }
    }

    [Net.SkipHandleWhileLoading]
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
                if (data.smallId != PlayerIdManager.LocalSmallId && PlayerRep.Representations.TryGetValue(data.smallId, out var rep) && rep.IsCreated) {
                    rep.repControllerRig.feetOffset = data.feetOffset;
                    rep.serializedLocalTransforms = data.serializedLocalTransforms;
                    rep.serializedPelvis = data.serializedPelvis;
                    rep.serializedFootball = data.serializedFootball;
                    rep.repPlayspace.rotation = data.serializedPlayspace.Expand();
                    rep.predictVelocity = data.predictVelocity.Expand();
                    rep.timeSincePelvisSent = Time.realtimeSinceStartup;

                    data.leftHand.CopyTo(rep.repLeftController);
                    data.rightHand.CopyTo(rep.repRightController);
                }
            }
        }
    }
}
