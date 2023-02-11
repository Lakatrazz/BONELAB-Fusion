using LabFusion.Data;
using LabFusion.Exceptions;
using LabFusion.Representation;
using LabFusion.Utilities;
using SLZ.Marrow.SceneStreaming;
using System;
using UnityEngine;

namespace LabFusion.Network
{
    public class LevelRequestData : IFusionSerializable, IDisposable
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

        public void Dispose() {
            GC.SuppressFinalize(this);
        }

        public static LevelRequestData Create(byte smallId, string barcode, string title) {
            return new LevelRequestData() {
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
            if (Time.realtimeSinceStartup - _timeOfRequest <= _requestCooldown)
                return;

            _timeOfRequest = Time.realtimeSinceStartup;

            if (NetworkInfo.IsServer && isServerHandled) {
                using (var reader = FusionReader.Create(bytes)) {
                    using (var data = reader.ReadFusionSerializable<LevelRequestData>()) {

                        // Get player and their username
                        var id = PlayerIdManager.GetPlayerId(data.smallId);

                        if (id != null && id.TryGetDisplayName(out var name)) {
                            FusionNotifier.Send(new FusionNotification() {
                                title = $"{data.title} Load Request",
                                message = $"{name} has requested to load {data.title}.",
                                messageColor = Color.yellow,
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
                }
            }
            else
                throw new ExpectedServerException();
        }
    }
}
