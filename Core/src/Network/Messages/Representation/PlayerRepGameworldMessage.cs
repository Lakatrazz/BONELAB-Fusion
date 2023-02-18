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
    public class PlayerRepGameworldData : IFusionSerializable, IDisposable
    {
        public const int Size = sizeof(byte) + SerializedLocalTransform.Size * RigAbstractor.GameworldRigTransformCount;

        public byte smallId;
        public SerializedLocalTransform[] serializedGameworldLocalTransforms = new SerializedLocalTransform[RigAbstractor.GameworldRigTransformCount];


        public void Serialize(FusionWriter writer)
        {
            writer.Write(smallId);
            for (var i = 0; i < RigAbstractor.GameworldRigTransformCount; i++)
                writer.Write(serializedGameworldLocalTransforms[i]);
        }

        public void Deserialize(FusionReader reader)
        {
            smallId = reader.ReadByte();
            for (var i = 0; i < RigAbstractor.GameworldRigTransformCount; i++)
                serializedGameworldLocalTransforms[i] = reader.ReadFusionSerializable<SerializedLocalTransform>();
        }

        public void Dispose() { 
            GC.SuppressFinalize(this);
        }

        public static PlayerRepGameworldData Create(byte smallId, Transform[] syncTransforms)
        {
            var data = new PlayerRepGameworldData
            {
                smallId = smallId,
            };

            for (var i = 0; i < RigAbstractor.GameworldRigTransformCount; i++) {
                data.serializedGameworldLocalTransforms[i] = new SerializedLocalTransform(syncTransforms[i]);
            }

            return data;
        }
    }

    [Net.SkipHandleWhileLoading]
    public class PlayerRepGameworldMessage : FusionMessageHandler {
        public override byte? Tag => NativeMessageTag.PlayerRepGameworld;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false) {
            using (var reader = FusionReader.Create(bytes)) {
                var data = reader.ReadFusionSerializable<PlayerRepGameworldData>();

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
                    rep.serializedGameworldLocalTransforms = data.serializedGameworldLocalTransforms;
                }
            }
        }
    }
}
