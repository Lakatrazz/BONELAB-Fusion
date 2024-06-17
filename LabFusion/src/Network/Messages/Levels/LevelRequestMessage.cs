using LabFusion.Data;
using LabFusion.Exceptions;
using LabFusion.Representation;
using LabFusion.Utilities;
using Il2CppSLZ.Marrow.SceneStreaming;
using UnityEngine;

namespace LabFusion.Network
{
    public class LevelRequestData : IFusionSerializable
    {
        public byte smallId;
        public string barcode;
        public string title;

        public void Serialize(FusionWriter writer)
        {
            writer.Write(smallId);
            writer.Write(barcode);
            writer.Write(title);
        }

        public void Deserialize(FusionReader reader)
        {
            smallId = reader.ReadByte();
            barcode = reader.ReadString();
            title = reader.ReadString();
        }

        public static LevelRequestData Create(byte smallId, string barcode, string title)
        {
            return new LevelRequestData()
            {
                smallId = smallId,
                barcode = barcode,
                title = title,
            };
        }
    }

    public class LevelRequestMessage : FusionMessageHandler
    {
        private const float _requestCooldown = 10f;
        private static float _timeOfRequest = -1000f;

        public override byte? Tag => NativeMessageTag.LevelRequest;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
        {
            // Prevent request spamming
            if (TimeUtilities.TimeSinceStartup - _timeOfRequest <= _requestCooldown)
                return;

            _timeOfRequest = TimeUtilities.TimeSinceStartup;

            if (isServerHandled)
            {
                using var reader = FusionReader.Create(bytes);
                var data = reader.ReadFusionSerializable<LevelRequestData>();

                // Get player and their username
                var id = PlayerIdManager.GetPlayerId(data.smallId);

                if (id != null && id.TryGetDisplayName(out var name))
                {
                    FusionNotifier.Send(new FusionNotification()
                    {
                        title = $"{data.title} Load Request",
                        message = new NotificationText($"{name} has requested to load {data.title}.", Color.yellow),

                        isMenuItem = true,
                        isPopup = true,
                        onCreateCategory = (c) =>
                        {
                            c.CreateFunctionElement($"Accept", Color.yellow, () =>
                            {
                                SceneStreamer.Load(data.barcode);
                            });
                        },
                    });
                }
            }
            else
                throw new ExpectedServerException();
        }
    }
}
