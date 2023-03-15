using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LabFusion.Data;
using LabFusion.Patching;

namespace LabFusion.Network
{
    public enum HolodeckEventType {
        UNKNOWN = 0,
        TOGGLE_DOOR = 1,
        SELECT_MATERIAL = 2,
    }

    public class HolodeckEventData : IFusionSerializable, IDisposable
    {
        public byte smallId;
        public HolodeckEventType type;
        public int selectionIndex;
        public bool toggleValue;

        public void Serialize(FusionWriter writer)
        {
            writer.Write(smallId);
            writer.Write((byte)type);
            writer.Write((byte)selectionIndex);
            writer.Write(toggleValue);
        }

        public void Deserialize(FusionReader reader)
        {
            smallId = reader.ReadByte();
            type = (HolodeckEventType)reader.ReadByte();
            selectionIndex = reader.ReadByte();
            toggleValue = reader.ReadBoolean();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public static HolodeckEventData Create(byte smallId, HolodeckEventType type, int selectionIndex, bool toggleValue)
        {
            return new HolodeckEventData()
            {
                smallId = smallId,
                type = type,
                selectionIndex = selectionIndex,
                toggleValue = toggleValue,
            };
        }
    }

    [Net.DelayWhileTargetLoading]
    public class HolodeckEventMessage : FusionMessageHandler
    {
        public override byte? Tag => NativeMessageTag.HolodeckEvent;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
        {
            using (FusionReader reader = FusionReader.Create(bytes))
            {
                using (var data = reader.ReadFusionSerializable<HolodeckEventData>())
                {
                    if (NetworkInfo.IsServer && isServerHandled) {
                        using (var message = FusionMessage.Create(Tag.Value, bytes))
                        {
                            MessageSender.BroadcastMessageExcept(data.smallId, NetworkChannel.Reliable, message, false);
                        }
                    }
                    else {
                        var deck = HolodeckData.GameController;

                        GameControl_HolodeckPatches.IgnorePatches = true;

                        if (deck != null)
                        {
                            switch (data.type)
                            {
                                default:
                                case HolodeckEventType.UNKNOWN:
                                    break;
                                case HolodeckEventType.TOGGLE_DOOR:
                                    deck.doorHide.SetActive(!data.toggleValue);
                                    deck.TOGGLEDOOR();
                                    break;
                                case HolodeckEventType.SELECT_MATERIAL:
                                    deck.SELECTMATERIAL(data.selectionIndex);
                                    break;
                            }
                        }

                        GameControl_HolodeckPatches.IgnorePatches = false;
                    }
                }
            }
        }
    }
}
