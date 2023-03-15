using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LabFusion.Data;
using LabFusion.Representation;
using LabFusion.Utilities;
using LabFusion.Grabbables;

using SLZ;
using SLZ.Interaction;
using System.Collections;
using UnityEngine;
using MelonLoader;

namespace LabFusion.Network
{
    public class PlayerRepGrabData : IFusionSerializable, IDisposable {
        public const int Size = sizeof(byte) * 3 + SerializedGrab.Size;

        public byte smallId;
        public Handedness handedness;
        public GrabGroup group;
        public SerializedGrab serializedGrab;

        public void Serialize(FusionWriter writer) {
            writer.Write(smallId);
            writer.Write((byte)handedness);
            writer.Write((byte)group);
            writer.Write(serializedGrab);
        }

        public void Deserialize(FusionReader reader) {
            smallId = reader.ReadByte();
            handedness = (Handedness)reader.ReadByte();
            group = (GrabGroup)reader.ReadByte();

            GrabGroupHandler.ReadGrab(ref serializedGrab, reader, group);
        }

        public Grip GetGrip() {
            return serializedGrab.GetGrip();
        }

        public PlayerRep GetRep() {
            if (PlayerRepManager.TryGetPlayerRep(smallId, out var rep))
                return rep;
            return null;
        }

        public void Dispose() {
            GC.SuppressFinalize(this);
        }

        public static PlayerRepGrabData Create(byte smallId, Handedness handedness, GrabGroup group, SerializedGrab serializedGrab) {
            return new PlayerRepGrabData() {
                smallId = smallId,
                handedness = handedness,
                group = group,
                serializedGrab = serializedGrab
            };
        }
    }

    [Net.DelayWhileTargetLoading]
    public class PlayerRepGrabMessage : FusionMessageHandler {
        public override byte? Tag => NativeMessageTag.PlayerRepGrab;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false) {
            using (FusionReader reader = FusionReader.Create(bytes)) {
                using (var data = reader.ReadFusionSerializable<PlayerRepGrabData>()) {

                    if (data.smallId != PlayerIdManager.LocalSmallId) {
                        MelonCoroutines.Start(CoWaitAndGrab(data));

                        // Send message to other clients if server
                        if (NetworkInfo.IsServer && isServerHandled) {
                            using (var message = FusionMessage.Create(Tag.Value, bytes)) {
                                MessageSender.BroadcastMessageExcept(data.smallId, NetworkChannel.Reliable, message);
                            }
                        }
                    }
                }
            }
        }

        private IEnumerator CoWaitAndGrab(PlayerRepGrabData data) {
            var rep = data.GetRep();

            if (rep == null)
                yield break;

            var grip = data.GetGrip();
            if (grip == null) {
                float time = Time.realtimeSinceStartup;
                while (grip == null && (Time.realtimeSinceStartup - time) <= 0.5f) {
                    yield return null;
                    grip = data.GetGrip();
                }

                if (grip == null)
                    yield break;
            }

            data.serializedGrab.RequestGrab(rep, data.handedness, grip);
        }
    }
}
