﻿using LabFusion.Data;
using LabFusion.SDK.Points;
using LabFusion.Player;
using LabFusion.Extensions;

namespace LabFusion.Network
{
    public class PointItemTriggerValueData : IFusionSerializable
    {
        public const int DefaultSize = sizeof(byte);

        public byte smallId;
        public string barcode;
        public string value;

        public static int GetSize(string barcode, string value)
        {
            return DefaultSize + barcode.GetSize() + value.GetSize();
        }

        public void Serialize(FusionWriter writer)
        {
            writer.Write(smallId);
            writer.Write(barcode);
            writer.Write(value);
        }

        public void Deserialize(FusionReader reader)
        {
            smallId = reader.ReadByte();
            barcode = reader.ReadString();
            value = reader.ReadString();
        }

        public static PointItemTriggerValueData Create(byte smallId, string barcode, string value)
        {
            return new PointItemTriggerValueData()
            {
                smallId = smallId,
                barcode = barcode,
                value = value,
            };
        }
    }

    public class PointItemTriggerValueMessage : FusionMessageHandler
    {
        public override byte Tag => NativeMessageTag.PointItemTriggerValue;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
        {
            using var reader = FusionReader.Create(bytes);
            var data = reader.ReadFusionSerializable<PointItemTriggerValueData>();
            // Send message to other clients if server
            if (isServerHandled)
            {
                using var message = FusionMessage.Create(Tag, bytes);
                MessageSender.BroadcastMessageExcept(data.smallId, NetworkChannel.Reliable, message, false);
            }
            else
            {
                var id = PlayerIdManager.GetPlayerId(data.smallId);
                PointItemManager.Internal_OnTriggerItem(id, data.barcode, data.value);
            }
        }
    }
}
