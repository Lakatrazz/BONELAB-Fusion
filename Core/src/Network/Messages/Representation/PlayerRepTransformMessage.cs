using LabFusion.Data;
using LabFusion.Extensions;
using LabFusion.Representation;
using LabFusion.Utilities;

using SLZ.Interaction;
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
        public const int Size = sizeof(byte) * 4 + sizeof(float) * 10 + SerializedLocalTransform.Size 
            * RigAbstractor.TransformSyncCount + SerializedTransform.Size + SerializedQuaternion.Size + SerializedHand.Size * 2;

        public byte smallId;

        public float feetOffset;
        public float crouchTarget;
        public float spineCrouchOff;

        public ControllerRig.TraversalState travState;
        public ControllerRig.VertState vertState;
        public ControllerRig.VrVertState vrVertState;

        public float curr_Health;

        public SerializedLocalTransform[] serializedLocalTransforms = new SerializedLocalTransform[RigAbstractor.TransformSyncCount];
        public SerializedTransform serializedPelvis;
        public SerializedSmallQuaternion serializedPlayspace;

        public Vector3 predictVelocity;
        public Vector3 predictAngularVelocity;

        public SerializedHand leftHand;
        public SerializedHand rightHand;


        public void Serialize(FusionWriter writer)
        {
            writer.Write(smallId);

            writer.Write(predictVelocity);
            writer.Write(predictAngularVelocity);

            writer.Write(feetOffset);
            writer.Write(crouchTarget);
            writer.Write(spineCrouchOff);

            writer.Write((byte)travState);
            writer.Write((byte)vertState);
            writer.Write((byte)vrVertState);

            writer.Write(curr_Health);

            for (var i = 0; i < RigAbstractor.TransformSyncCount; i++)
                writer.Write(serializedLocalTransforms[i]);

            writer.Write(serializedPelvis);
            writer.Write(serializedPlayspace);

            writer.Write(leftHand);
            writer.Write(rightHand);
        }

        public void Deserialize(FusionReader reader)
        {
            smallId = reader.ReadByte();

            predictVelocity = reader.ReadVector3();
            predictAngularVelocity = reader.ReadVector3();

            feetOffset = reader.ReadSingle();
            crouchTarget = reader.ReadSingle();
            spineCrouchOff = reader.ReadSingle();

            travState = (ControllerRig.TraversalState)reader.ReadByte();
            vertState = (ControllerRig.VertState)reader.ReadByte();
            vrVertState = (ControllerRig.VrVertState)reader.ReadByte();

            curr_Health = reader.ReadSingle();

            for (var i = 0; i < RigAbstractor.TransformSyncCount; i++)
                serializedLocalTransforms[i] = reader.ReadFusionSerializable<SerializedLocalTransform>();

            serializedPelvis = reader.ReadFusionSerializable<SerializedTransform>();
            serializedPlayspace = reader.ReadFusionSerializable<SerializedSmallQuaternion>();

            leftHand = reader.ReadFusionSerializable<SerializedHand>();
            rightHand = reader.ReadFusionSerializable<SerializedHand>();
        }

        public void Dispose() { 
            GC.SuppressFinalize(this);
        }

        public static PlayerRepTransformData Create(byte smallId, Transform[] syncTransforms, Transform syncedPelvis, Transform syncedPlayspace, Hand leftHand, Hand rightHand)
        {
            var rm = RigData.RigReferences.RigManager;
            var health = RigData.RigReferences.Health;
            var controllerRig = rm.openControllerRig;

            var data = new PlayerRepTransformData {
                smallId = smallId,

                predictVelocity = RigData.RigReferences.RigManager.physicsRig.torso._pelvisRb.velocity * Time.timeScale,
                predictAngularVelocity = RigData.RigReferences.RigManager.physicsRig.torso._pelvisRb.angularVelocity * Time.timeScale,

                feetOffset = controllerRig.feetOffset,
                crouchTarget = controllerRig._crouchTarget,
                travState = controllerRig.travState,
                vertState = controllerRig.vertState,
                vrVertState = controllerRig.vrVertState,

                curr_Health = health.curr_Health,

                serializedPelvis = new SerializedTransform(syncedPelvis),

                serializedPlayspace = SerializedSmallQuaternion.Compress(syncedPlayspace.rotation),

                leftHand = new SerializedHand(leftHand, leftHand.Controller),
                rightHand = new SerializedHand(rightHand, rightHand.Controller)
            };

            for (var i = 0; i < RigAbstractor.TransformSyncCount; i++) {
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
                if (data.smallId != PlayerIdManager.LocalSmallId && PlayerRepManager.TryGetPlayerRep(data.smallId, out var rep) && rep.IsCreated) {
                    rep.serializedFeetOffset = data.feetOffset;
                    rep.serializedCrouchTarget = data.crouchTarget;
                    rep.serializedSpineCrouchOff = data.spineCrouchOff;

                    rep.serializedTravState = data.travState;
                    rep.serializedVertState = data.vertState;
                    rep.serializedVrVertState = data.vrVertState;

                    rep.serializedLocalTransforms = data.serializedLocalTransforms;
                    rep.serializedPelvis = data.serializedPelvis;
                    rep.repPlayspace.rotation = data.serializedPlayspace.Expand();
                    rep.predictVelocity = data.predictVelocity;
                    rep.predictAngularVelocity = data.predictAngularVelocity;
                    rep.timeSincePelvisSent = Time.realtimeSinceStartup;

                    rep.serializedLeftHand = data.leftHand;
                    rep.serializedRightHand = data.rightHand;

                    // Apply changes to the RigManager
                    if (rep.IsCreated) {
                        var health = rep.RigReferences.Health;
                        health.curr_Health = data.curr_Health;
                    }
                }
            }
        }
    }
}
