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
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

using SystemVector3 = System.Numerics.Vector3;

namespace LabFusion.Network
{
    public unsafe class PlayerRepTransformData : IFusionSerializable, IDisposable
    {
        public const int Size = sizeof(byte) + sizeof(float) * 7 + SerializedLocalTransform.Size
            * RigAbstractor.TransformSyncCount + SerializedTransform.Size + SerializedSmallQuaternion.Size + SerializedHand.Size * 2;

        public byte smallId;

        public float curr_Health;

        public SerializedLocalTransform* serializedLocalTransforms;
        public SerializedTransform serializedPelvis;
        public SerializedSmallQuaternion serializedPlayspace;

        public SystemVector3 predictVelocity;
        public SystemVector3 predictAngularVelocity;

        public SerializedHand leftHand;
        public SerializedHand rightHand;

        private bool _disposed;

        public void Serialize(FusionWriter writer)
        {
            writer.Write(smallId);

            writer.Write(predictVelocity);
            writer.Write(predictAngularVelocity);

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

            predictVelocity = reader.ReadSystemVector3();
            predictAngularVelocity = reader.ReadSystemVector3();

            curr_Health = reader.ReadSingle();

            serializedLocalTransforms = (SerializedLocalTransform*)Marshal.AllocHGlobal(RigAbstractor.TransformSyncCount * sizeof(SerializedLocalTransform));

            for (var i = 0; i < RigAbstractor.TransformSyncCount; i++)
                serializedLocalTransforms[i] = reader.ReadFusionSerializable<SerializedLocalTransform>();

            serializedPelvis = reader.ReadFusionSerializable<SerializedTransform>();
            serializedPlayspace = reader.ReadFromFactory(SerializedSmallQuaternion.Create);

            leftHand = reader.ReadFusionSerializable<SerializedHand>();
            rightHand = reader.ReadFusionSerializable<SerializedHand>();
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            GC.SuppressFinalize(this);
            Marshal.FreeHGlobal((IntPtr)serializedLocalTransforms);

            _disposed = true;
        }

        public static PlayerRepTransformData Create(byte smallId, Transform[] syncTransforms, Transform syncedPelvis, Transform syncedPlayspace, Hand leftHand, Hand rightHand)
        {
            var health = RigData.RigReferences.Health;

            var data = new PlayerRepTransformData
            {
                smallId = smallId,

                predictVelocity = RigData.RigReferences.RigManager.physicsRig.torso._pelvisRb.velocity.ToSystemVector3() * TimeUtilities.TimeScale,
                predictAngularVelocity = RigData.RigReferences.RigManager.physicsRig.torso._pelvisRb.angularVelocity.ToSystemVector3() * TimeUtilities.TimeScale,

                curr_Health = health.curr_Health,

                serializedPelvis = new SerializedTransform(syncedPelvis),

                serializedPlayspace = SerializedSmallQuaternion.Compress(syncedPlayspace.rotation.ToSystemQuaternion()),

                leftHand = new SerializedHand(leftHand, leftHand.Controller),
                rightHand = new SerializedHand(rightHand, rightHand.Controller),

                serializedLocalTransforms = (SerializedLocalTransform*)Marshal.AllocHGlobal(RigAbstractor.TransformSyncCount * sizeof(SerializedLocalTransform)),
            };

            for (var i = 0; i < RigAbstractor.TransformSyncCount; i++)
            {
                data.serializedLocalTransforms[i] = new SerializedLocalTransform(syncTransforms[i]);
            }

            return data;
        }
    }

    [Net.SkipHandleWhileLoading]
    public class PlayerRepTransformMessage : FusionMessageHandler
    {
        public override byte? Tag => NativeMessageTag.PlayerRepTransform;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
        {
            using var reader = FusionReader.Create(bytes);
            var data = reader.ReadFusionSerializable<PlayerRepTransformData>();

            // Send message to other clients if server
            if (NetworkInfo.IsServer && isServerHandled)
            {
                using var message = FusionMessage.Create(Tag.Value, bytes);
                MessageSender.BroadcastMessageExcept(data.smallId, NetworkChannel.Unreliable, message);
            }

            // Apply player rep data
            if (data.smallId != PlayerIdManager.LocalSmallId && PlayerRepManager.TryGetPlayerRep(data.smallId, out var rep) && rep.IsCreated)
            {
                unsafe {
                    for (var i = 0; i < RigAbstractor.TransformSyncCount; i++) {
                        rep.serializedLocalTransforms[i] = data.serializedLocalTransforms[i];
                    }
                }

                rep.serializedPelvis = data.serializedPelvis;
                rep.repPlayspace.rotation = data.serializedPlayspace.Expand().ToUnityQuaternion();
                rep.predictVelocity = data.predictVelocity;
                rep.predictAngularVelocity = data.predictAngularVelocity;
                rep.timeSincePelvisSent = TimeUtilities.TimeSinceStartup;

                rep.serializedLeftHand = data.leftHand;
                rep.serializedRightHand = data.rightHand;

                // Apply changes to the RigManager
                if (rep.IsCreated)
                {
                    var health = rep.RigReferences.Health;
                    health.curr_Health = data.curr_Health;
                }
            }
        }
    }
}
