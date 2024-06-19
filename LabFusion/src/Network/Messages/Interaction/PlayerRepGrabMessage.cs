using LabFusion.Data;
using LabFusion.Representation;
using LabFusion.Utilities;
using LabFusion.Grabbables;
using Il2CppSLZ.Interaction;
using System.Collections;
using MelonLoader;
using Il2CppSLZ.Marrow.Interaction;
using LabFusion.Entities;

namespace LabFusion.Network
{
    public class PlayerRepGrabData : IFusionSerializable
    {
        public const int Size = sizeof(byte) * 3;

        public byte smallId;
        public Handedness handedness;
        public GrabGroup group;
        public SerializedGrab serializedGrab;

        public void Serialize(FusionWriter writer)
        {
            writer.Write(smallId);
            writer.Write((byte)handedness);
            writer.Write((byte)group);
            writer.Write(serializedGrab);
        }

        public void Deserialize(FusionReader reader)
        {
            smallId = reader.ReadByte();
            handedness = (Handedness)reader.ReadByte();
            group = (GrabGroup)reader.ReadByte();

            GrabGroupHandler.ReadGrab(ref serializedGrab, reader, group);
        }

        public Grip GetGrip()
        {
            return serializedGrab.GetGrip();
        }

        public NetworkPlayer GetPlayer()
        {
            if (NetworkPlayerManager.TryGetPlayer(smallId, out var player))
                return player;
            return null;
        }

        public static PlayerRepGrabData Create(byte smallId, Handedness handedness, GrabGroup group, SerializedGrab serializedGrab)
        {
            return new PlayerRepGrabData()
            {
                smallId = smallId,
                handedness = handedness,
                group = group,
                serializedGrab = serializedGrab
            };
        }
    }

    [Net.DelayWhileTargetLoading]
    public class PlayerRepGrabMessage : FusionMessageHandler
    {
        public override byte? Tag => NativeMessageTag.PlayerRepGrab;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
        {
            using FusionReader reader = FusionReader.Create(bytes);
            var data = reader.ReadFusionSerializable<PlayerRepGrabData>();

            if (data.smallId != PlayerIdManager.LocalSmallId)
            {
                MelonCoroutines.Start(CoWaitAndGrab(data));

                // Send message to other clients if server
                if (isServerHandled)
                {
                    using var message = FusionMessage.Create(Tag.Value, bytes);
                    MessageSender.BroadcastMessageExcept(data.smallId, NetworkChannel.Reliable, message);
                }
            }
        }

        private IEnumerator CoWaitAndGrab(PlayerRepGrabData data)
        {
            var player = data.GetPlayer();

            if (player == null)
                yield break;

            var grip = data.GetGrip();
            if (grip == null)
            {
                float time = TimeUtilities.TimeSinceStartup;
                while (grip == null && (TimeUtilities.TimeSinceStartup - time) <= 0.5f)
                {
                    yield return null;
                    grip = data.GetGrip();
                }

                if (grip == null)
                    yield break;
            }

            data.serializedGrab.RequestGrab(player, data.handedness, grip);
        }
    }
}
