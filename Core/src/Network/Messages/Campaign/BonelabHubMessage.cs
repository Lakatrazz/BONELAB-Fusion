using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LabFusion.Data;
using LabFusion.Representation;
using LabFusion.Utilities;
using LabFusion.Grabbables;
using LabFusion.Syncables;
using LabFusion.Patching;

using SLZ;
using SLZ.Interaction;
using SLZ.Props.Weapons;

namespace LabFusion.Network
{
    // jesus christ Hub has so many events I need to sync
    public enum BonelabHubEventType {
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

    public class BonelabHubEventData : IFusionSerializable, IDisposable
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

        public void Dispose()
        {
            GC.SuppressFinalize(this);
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
            using (FusionReader reader = FusionReader.Create(bytes))
            {
                using (var data = reader.ReadFusionSerializable<BonelabHubEventData>())
                {
                    GameControl_HubPatches.IgnorePatches = true;
                    var controller = HubData.GameController;

                    if (!NetworkInfo.IsServer && controller) {
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
                                controller.AIRLOCKOCCUPIED();
                                break;
                            case BonelabHubEventType.AIR_LOCK_UNOCCUPIED:
                                controller.AIRLOCKUNOCCUPIED();
                                break;
                            case BonelabHubEventType.AIR_LOCK_CYCLE:
                                controller.AIRLOCKCYCLE();
                                break;
                            case BonelabHubEventType.CANCEL_CYCLE:
                                controller.CANCELCYCLE();
                                break;
                            case BonelabHubEventType.OPEN_SMALL_DOOR:
                                controller.OpenSmallDoor();
                                break;
                            case BonelabHubEventType.CLOSE_SMALL_DOOR:
                                controller.CloseSmallDoor();
                                break;
                            case BonelabHubEventType.OPEN_BIG_DOORS:
                                controller.OpenBigDoors();
                                break;
                            case BonelabHubEventType.CLOSE_BIG_DOORS:
                                controller.CloseBigDoors();
                                break;
                        }
                    }

                    GameControl_HubPatches.IgnorePatches = false;
                }
            }
        }
    }
}
