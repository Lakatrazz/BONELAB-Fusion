using System;
using System.Runtime.InteropServices;
using LabFusion.Data;
using LabFusion.Representation;
using UnityEngine;

namespace LabFusion.Network
{
    public unsafe class PlayerRepGameworldData : IFusionSerializable, IDisposable
    {
        public const int Size = sizeof(byte) + SerializedLocalTransform.Size * RigAbstractor.GameworldRigTransformCount;

        public byte smallId;
        public SerializedLocalTransform* serializedGameworldLocalTransforms;

        private bool _disposed;

        public void Serialize(FusionWriter writer)
        {
            writer.Write(smallId);
            for (var i = 0; i < RigAbstractor.GameworldRigTransformCount; i++)
                writer.Write(serializedGameworldLocalTransforms[i]);
        }

        public void Deserialize(FusionReader reader)
        {
            smallId = reader.ReadByte();

            serializedGameworldLocalTransforms = (SerializedLocalTransform*)Marshal.AllocHGlobal(RigAbstractor.GameworldRigTransformCount * sizeof(SerializedLocalTransform));

            for (var i = 0; i < RigAbstractor.GameworldRigTransformCount; i++)
                serializedGameworldLocalTransforms[i] = reader.ReadFusionSerializable<SerializedLocalTransform>();
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            GC.SuppressFinalize(this);
            Marshal.FreeHGlobal((IntPtr)serializedGameworldLocalTransforms);

            _disposed = true;
        }

        public static PlayerRepGameworldData Create(byte smallId, Transform[] syncTransforms)
        {
            var data = new PlayerRepGameworldData
            {
                smallId = smallId,

                serializedGameworldLocalTransforms = (SerializedLocalTransform*)Marshal.AllocHGlobal(RigAbstractor.GameworldRigTransformCount * sizeof(SerializedLocalTransform))
            };

            for (var i = 0; i < RigAbstractor.GameworldRigTransformCount; i++)
            {
                data.serializedGameworldLocalTransforms[i] = new SerializedLocalTransform(syncTransforms[i]);
            }

            return data;
        }
    }

    [Net.SkipHandleWhileLoading]
    public class PlayerRepGameworldMessage : FusionMessageHandler
    {
        public override byte? Tag => NativeMessageTag.PlayerRepGameworld;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
        {
            using var reader = FusionReader.Create(bytes);
            var data = reader.ReadFusionSerializable<PlayerRepGameworldData>();

            // Send message to other clients if server
            if (NetworkInfo.IsServer && isServerHandled)
            {
                if (data.smallId != 0)
                {
                    using var message = FusionMessage.Create(Tag.Value, bytes);
                    MessageSender.BroadcastMessageExcept(data.smallId, NetworkChannel.Unreliable, message);
                }
            }

            // Apply player rep data
            if (data.smallId != PlayerIdManager.LocalSmallId && PlayerRepManager.TryGetPlayerRep(data.smallId, out var rep) && rep.IsCreated)
            {
                unsafe {
                    for (var i = 0; i < RigAbstractor.GameworldRigTransformCount; i++) {
                        rep.serializedGameworldLocalTransforms[i] = data.serializedGameworldLocalTransforms[i];
                    }
                }
            }
        }
    }
}
