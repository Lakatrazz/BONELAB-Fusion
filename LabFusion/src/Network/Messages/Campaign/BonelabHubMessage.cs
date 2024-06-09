using LabFusion.Data;
using LabFusion.Patching;

namespace LabFusion.Network
{
    // jesus christ Hub has so many events I need to sync
    public enum BonelabHubEventType
    {
        UNKNOWN = 0,
        ELEVATOR_BREAKOUT = 1,
        SETUP_ELEVATOR = 2,
        OPEN_BW_DOOR = 3,
        BW_BOX_DESTROYED = 4,
        AIR_LOCK_ENTER_NORTH = 5,
        AIR_LOCK_ENTER_SOUTH = 6,
        AIR_LOCK_OCCUPIED = 7,
        AIR_LOCK_UNOCCUPIED = 8,
        AIR_LOCK_CYCLE = 9,
        CANCEL_CYCLE = 10,
        OPEN_SMALL_DOOR = 11,
        CLOSE_SMALL_DOOR = 12,
        OPEN_BIG_DOORS = 13,
        CLOSE_BIG_DOORS = 14,
    }

    public class BonelabHubEventData : IFusionSerializable
    {
        public BonelabHubEventType type;

        public void Serialize(FusionWriter writer)
        {
            writer.Write((byte)type);
        }

        public void Deserialize(FusionReader reader)
        {
            type = (BonelabHubEventType)reader.ReadByte();
        }

        public static BonelabHubEventData Create(BonelabHubEventType type)
        {
            return new BonelabHubEventData()
            {
                type = type,
            };
        }
    }

    [Net.DelayWhileTargetLoading]
    public class BonelabHubEventMessage : FusionMessageHandler
    {
        public override byte? Tag => NativeMessageTag.BonelabHubEvent;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
        {
            using FusionReader reader = FusionReader.Create(bytes);
            var data = reader.ReadFusionSerializable<BonelabHubEventData>();
            GameControl_HubPatches.IgnorePatches = true;
            var controller = HubData.GameController;

            if (!NetworkInfo.IsServer && controller)
            {
                switch (data.type)
                {
                    default:
                    case BonelabHubEventType.UNKNOWN:
                        break;
                    case BonelabHubEventType.ELEVATOR_BREAKOUT:
                        controller.ELEVATORBREAKOUT();
                        break;
                    case BonelabHubEventType.SETUP_ELEVATOR:
                        controller.SETUPELEVATOR();
                        break;
                    case BonelabHubEventType.OPEN_BW_DOOR:
                        controller.OPENBWDOOR();
                        break;
                    case BonelabHubEventType.BW_BOX_DESTROYED:
                        controller.BWBOXDESTROYED();
                        break;
                    case BonelabHubEventType.AIR_LOCK_ENTER_NORTH:
                        controller.AIRLOCKENTERNORTH();
                        break;
                    case BonelabHubEventType.AIR_LOCK_ENTER_SOUTH:
                        controller.AIRLOCKENTERSOUTH();
                        break;
                    case BonelabHubEventType.AIR_LOCK_OCCUPIED:
                        controller.AIRLOCKOCCUPIED(true);
                        break;
                    case BonelabHubEventType.AIR_LOCK_UNOCCUPIED:
                        controller.AIRLOCKOCCUPIED(false);
                        break;
                    case BonelabHubEventType.AIR_LOCK_CYCLE:
                        controller.AIRLOCKCYCLE();
                        break;
                    case BonelabHubEventType.CANCEL_CYCLE:
                        controller.CANCELCYCLE();
                        break;
                    case BonelabHubEventType.OPEN_SMALL_DOOR:
                        controller.OpenSmallDoor(null);
                        break;
                    case BonelabHubEventType.CLOSE_SMALL_DOOR:
                        controller.CloseSmallDoor(null);
                        break;
                    case BonelabHubEventType.OPEN_BIG_DOORS:
                        controller.OpenBigDoors(null);
                        break;
                    case BonelabHubEventType.CLOSE_BIG_DOORS:
                        controller.CloseBigDoors(null);
                        break;
                }
            }

            GameControl_HubPatches.IgnorePatches = false;
        }
    }
}
