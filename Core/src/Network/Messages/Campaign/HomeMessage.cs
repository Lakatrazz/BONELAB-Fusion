using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LabFusion.Data;
using LabFusion.Patching;
using LabFusion.Utilities;

namespace LabFusion.Network
{
    public enum HomeEventType
    {
        UNKNOWN = 0,
        WARMUP_JIMMY_ARM = 1,
        REACH_WINDMILL = 2,
        REACHED_TAXI = 3,
        ARM_HIDE = 4,
        VOID_DRIVING = 5,
        DRIVING_END = 6,
        COMPLETE_GAME = 7,
        SEQUENCE_PROGRESS = 8,
        SPLINE_LOOP_COUNTER = 9,
    }

    public class HomeEventData : IFusionSerializable, IDisposable
    {
        public byte selectionNumber;
        public HomeEventType type;

        public void Serialize(FusionWriter writer)
        {
            writer.Write(selectionNumber);
            writer.Write((byte)type);
        }

        public void Deserialize(FusionReader reader)
        {
            selectionNumber = reader.ReadByte();
            type = (HomeEventType)reader.ReadByte();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public static HomeEventData Create(byte selectionNumber, HomeEventType type)
        {
            return new HomeEventData()
            {
                selectionNumber = selectionNumber,
                type = type,
            };
        }
    }

    [Net.DelayWhileTargetLoading]
    public class HomeEventMessage : FusionMessageHandler
    {
        public override byte? Tag => NativeMessageTag.HomeEvent;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
        {
            using (FusionReader reader = FusionReader.Create(bytes))
            {
                using (var data = reader.ReadFusionSerializable<HomeEventData>())
                {
                    var controller = HomeData.GameController;
                    HomePatches.IgnorePatches = true;
                    TaxiControllerPatches.IgnorePatches = true;

                    // We ONLY handle this for clients, this message should only ever be sent by the server!
                    if (!NetworkInfo.IsServer && controller)
                    {
                        switch (data.type)
                        {
                            default:
                            case HomeEventType.UNKNOWN:
                                break;
                            case HomeEventType.WARMUP_JIMMY_ARM:
                                controller.WarmUpJimmyArm();
                                break;
                            case HomeEventType.REACH_WINDMILL:
                                controller.ReachWindmill();
                                break;
                            case HomeEventType.REACHED_TAXI:
                                controller.ReachedTaxi();
                                break;
                            case HomeEventType.ARM_HIDE:
                                controller.ArmHide();

                                HomeData.TeleportToJimmyFinger();
                                break;
                            case HomeEventType.VOID_DRIVING:
                                controller.VoidDriving();
                                break;
                            case HomeEventType.DRIVING_END:
                                controller.DrivingEnd();
                                break;
                            case HomeEventType.COMPLETE_GAME:
                                controller.CompleteGame();
                                break;
                            case HomeEventType.SEQUENCE_PROGRESS:
                                controller.SequenceProgress(data.selectionNumber);
                                break;
                            case HomeEventType.SPLINE_LOOP_COUNTER:
                                HomeData.TaxiController.SplineLoopCounter();
                                break;
                        }
                    }

                    HomePatches.IgnorePatches = false;
                    TaxiControllerPatches.IgnorePatches = false;
                }
            }
        }
    }
}
