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
    public enum DescentElevatorType {
        UNKNOWN = 0,
        START_ELEVATOR = 1,
        STOP_ELEVATOR = 2,
        SEAL_DOORS = 3,
        START_MOVE_UPWARD = 4,
        SLOW_UPWARD_MOVEMENT = 5,
        OPEN_DOORS = 6,
        CLOSE_DOORS = 7,
    }

    public class DescentElevatorData : IFusionSerializable, IDisposable
    {
        public byte smallId;
        public DescentElevatorType type;

        public void Serialize(FusionWriter writer)
        {
            writer.Write(smallId);
            writer.Write((byte)type);
        }

        public void Deserialize(FusionReader reader)
        {
            smallId = reader.ReadByte();
            type = (DescentElevatorType)reader.ReadByte();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public static DescentElevatorData Create(byte smallId, DescentElevatorType type)
        {
            return new DescentElevatorData()
            {
                smallId = smallId,
                type = type,
            };
        }
    }

    [Net.DelayWhileTargetLoading]
    public class DescentElevatorMessage : FusionMessageHandler
    {
        public override byte? Tag => NativeMessageTag.DescentElevator;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
        {
            using (FusionReader reader = FusionReader.Create(bytes))
            {
                using (var data = reader.ReadFusionSerializable<DescentElevatorData>())
                {
                    // Send message to other clients if server
                    if (NetworkInfo.IsServer && isServerHandled) {
                        using (var message = FusionMessage.Create(Tag.Value, bytes)) {
                            MessageSender.BroadcastMessageExcept(data.smallId, NetworkChannel.Reliable, message, false);
                        }
                    }
                    else {
                        if (!DescentData.Elevator)
                            DescentData.OnCacheInfo();

                        ElevatorPatches.IgnorePatches = true;

                        switch (data.type) {
                            default:
                            case DescentElevatorType.UNKNOWN:
                                break;
                            case DescentElevatorType.START_ELEVATOR:
                                DescentData.Elevator.StartElevator();
                                break;
                            case DescentElevatorType.STOP_ELEVATOR:
                                DescentData.Elevator.StopDoorRoutine();
                                break;
                            case DescentElevatorType.SEAL_DOORS:
                                DescentData.Elevator.SealDoors();
                                break;
                            case DescentElevatorType.START_MOVE_UPWARD:
                                DescentData.Elevator.StartMoveUpward();
                                break;
                            case DescentElevatorType.SLOW_UPWARD_MOVEMENT:
                                DescentData.Elevator.SlowUpwardMovement();
                                break;
                            case DescentElevatorType.OPEN_DOORS:
                                DescentData.Elevator.OpenDoors();
                                break;
                            case DescentElevatorType.CLOSE_DOORS:
                                DescentData.Elevator.CloseDoors();
                                break;
                        }

                        ElevatorPatches.IgnorePatches = false;
                    }
                }
            }
        }
    }
}
