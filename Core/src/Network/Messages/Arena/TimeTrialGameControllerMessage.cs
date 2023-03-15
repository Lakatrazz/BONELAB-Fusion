using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LabFusion.Data;
using LabFusion.Patching;

namespace LabFusion.Network
{
    public enum TimeTrialGameControllerType
    {
        UNKNOWN = 0,
        UpdateDifficulty = 1,
        TIMETRIAL_PlayerStartTrigger = 2,
        TIMETRIAL_PlayerEndTrigger = 3,
        ProgPointKillCount = 4,
        SetRequiredKillCount = 5,
    }

    public class TimeTrialGameControllerData : IFusionSerializable, IDisposable
    {
        public TimeTrialGameControllerType type;
        public byte value;

        public void Serialize(FusionWriter writer)
        {
            writer.Write((byte)type);
            writer.Write(value);
        }

        public void Deserialize(FusionReader reader)
        {
            type = (TimeTrialGameControllerType)reader.ReadByte();
            value = reader.ReadByte();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public static TimeTrialGameControllerData Create(TimeTrialGameControllerType type, int value)
        {
            return new TimeTrialGameControllerData()
            {
                type = type,
                value = (byte)value,
            };
        }
    }

    [Net.DelayWhileTargetLoading]
    public class TimeTrialGameControllerMessage : FusionMessageHandler
    {
        public override byte? Tag => NativeMessageTag.TimeTrial_GameController;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
        {
            using (FusionReader reader = FusionReader.Create(bytes))
            {
                using (var data = reader.ReadFusionSerializable<TimeTrialGameControllerData>())
                {
                    TimeTrial_GameControllerPatches.IgnorePatches = true;

                    // We ONLY handle this for clients, this message should only ever be sent by the server!
                    if (!NetworkInfo.IsServer && TimeTrialData.IsInTimeTrial)
                    {
                        switch (data.type)
                        {
                            default:
                            case TimeTrialGameControllerType.UNKNOWN:
                                break;
                            case TimeTrialGameControllerType.UpdateDifficulty:
                                TimeTrialData.GameController.UpdateDifficulty(data.value);
                                break;
                            case TimeTrialGameControllerType.TIMETRIAL_PlayerStartTrigger:
                                TimeTrialData.GameController.TIMETRIAL_PlayerStartTrigger();
                                break;
                            case TimeTrialGameControllerType.TIMETRIAL_PlayerEndTrigger:
                                TimeTrialData.GameController.TIMETRIAL_PlayerEndTrigger();
                                break;
                            case TimeTrialGameControllerType.ProgPointKillCount:
                                TimeTrialData.GameController.ProgPointKillCount(data.value);
                                break;
                            case TimeTrialGameControllerType.SetRequiredKillCount:
                                TimeTrialData.GameController.SetRequiredKillCount(data.value);
                                break;
                        }
                    }

                    TimeTrial_GameControllerPatches.IgnorePatches = false;
                }
            }
        }
    }
}
