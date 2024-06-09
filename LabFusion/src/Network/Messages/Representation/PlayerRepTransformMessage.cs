using LabFusion.Data;
using LabFusion.Representation;
using LabFusion.Utilities;

using Il2CppSLZ.Interaction;

using UnityEngine;



namespace LabFusion.Network
{
    public class PlayerRepTransformData : IFusionSerializable
    {
        public const int Size = sizeof(byte) + sizeof(float) * 7 + SerializedLocalTransform.Size
            * RigAbstractor.TransformSyncCount + SerializedTransform.Size + SerializedSmallQuaternion.Size + SerializedHand.Size * 2;

        public byte smallId;

        public float curr_Health;

        public SerializedLocalTransform[] serializedLocalTransforms;
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

            curr_Health = reader.ReadSingle();

            serializedLocalTransforms = new SerializedLocalTransform[RigAbstractor.TransformSyncCount];

            for (var i = 0; i < RigAbstractor.TransformSyncCount; i++)
                serializedLocalTransforms[i] = reader.ReadFusionSerializable<SerializedLocalTransform>();

            serializedPelvis = reader.ReadFusionSerializable<SerializedTransform>();
            serializedPlayspace = reader.ReadFusionSerializable<SerializedSmallQuaternion>();

            leftHand = reader.ReadFusionSerializable<SerializedHand>();
            rightHand = reader.ReadFusionSerializable<SerializedHand>();
        }

        public static PlayerRepTransformData Create(byte smallId, Transform[] syncTransforms, Transform syncedPelvis, Transform syncedPlayspace, Hand leftHand, Hand rightHand)
        {
            var health = RigData.RigReferences.Health;

            var data = new PlayerRepTransformData
            {
                smallId = smallId,

                predictVelocity = RigData.RigReferences.RigManager.physicsRig.torso._pelvisRb.velocity * TimeUtilities.TimeScale,
                predictAngularVelocity = RigData.RigReferences.RigManager.physicsRig.torso._pelvisRb.angularVelocity * TimeUtilities.TimeScale,

                curr_Health = health.curr_Health,

                serializedPelvis = new SerializedTransform(syncedPelvis),

                serializedPlayspace = SerializedSmallQuaternion.Compress(syncedPlayspace.rotation),

                leftHand = new SerializedHand(leftHand, leftHand.Controller),
                rightHand = new SerializedHand(rightHand, rightHand.Controller),

                serializedLocalTransforms = new SerializedLocalTransform[RigAbstractor.TransformSyncCount],
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
                for (var i = 0; i < RigAbstractor.TransformSyncCount; i++) {
                    rep.serializedLocalTransforms[i] = data.serializedLocalTransforms[i];
                }

                rep.serializedPelvis = data.serializedPelvis;
                rep.repPlayspace.rotation = data.serializedPlayspace.Expand();
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
