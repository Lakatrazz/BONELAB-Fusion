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
    public enum MagmaGateEventType {
        UNKNOWN = 0,
        BUTTONS_SETUP = 1,
        OBJECTIVE_COMPLETE_SETUP = 2,
        LOSE_SEQUENCE = 3,
        DOOR_DISSOLVE = 4,
    }

    public class MagmaGateEventData : IFusionSerializable, IDisposable
    {
        public MagmaGateEventType type;

        public void Serialize(FusionWriter writer)
        {
            writer.Write((byte)type);
        }

        public void Deserialize(FusionReader reader)
        {
            type = (MagmaGateEventType)reader.ReadByte();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public static MagmaGateEventData Create(MagmaGateEventType type)
        {
            return new MagmaGateEventData()
            {
                type = type,
            };
        }
    }

    [Net.DelayWhileTargetLoading]
    public class MagmaGateEventMessage : FusionMessageHandler
    {
        public override byte? Tag => NativeMessageTag.MagmaGateEvent;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
        {
            using (FusionReader reader = FusionReader.Create(bytes))
            {
                using (var data = reader.ReadFusionSerializable<MagmaGateEventData>())
                {
                    var controller = MagmaGateData.GameController;
                    MagmaGatePatches.IgnorePatches = true;

                    // We ONLY handle this for clients, this message should only ever be sent by the server!
                    if (!NetworkInfo.IsServer && controller)
                    {
                        switch (data.type)
                        {
                            default:
                            case MagmaGateEventType.UNKNOWN:
                                break;
                            case MagmaGateEventType.BUTTONS_SETUP:
                                controller.ButtonsSetup();
                                break;
                            case MagmaGateEventType.OBJECTIVE_COMPLETE_SETUP:
                                controller.ObjectiveCompleteSetup();
                                break;
                            case MagmaGateEventType.LOSE_SEQUENCE:
                                controller.LoseSequence();
                                break;
                            case MagmaGateEventType.DOOR_DISSOLVE:
                                controller.DoorDissolve();
                                break;
                        }
                    }

                    MagmaGatePatches.IgnorePatches = false;
                }
            }
        }
    }
}
