using LabFusion.Data;
using LabFusion.Patching;

namespace LabFusion.Network
{
    public enum HomeEventType
    {
        UNKNOWN = 0,
        WARMUP_JIMMY_ARM = 1,
        REACH_WINDMILL = 2,
        REACHED_TAXI = 3,
        ARM_HIDE = 4,
        DRIVING_END = 5,
        COMPLETE_GAME = 6,
        SPLINE_LOOP_COUNTER = 7,
    }

    public class HomeEventData : IFusionSerializable
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
            using FusionReader reader = FusionReader.Create(bytes);
            var data = reader.ReadFusionSerializable<HomeEventData>();
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
                    case HomeEventType.DRIVING_END:
                        controller.DrivingEnd();
                        break;
                    case HomeEventType.COMPLETE_GAME:
                        controller.CompleteGame();
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
