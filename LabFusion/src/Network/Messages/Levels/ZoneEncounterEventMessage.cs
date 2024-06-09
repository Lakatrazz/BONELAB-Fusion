using LabFusion.Data;
using LabFusion.Patching;

using UnityEngine;

using Il2CppSLZ.Marrow.Zones;

namespace LabFusion.Network
{
    public enum ZoneEncounterEventType
    {
        UNKNOWN = 0,
        COMPLETE_ENCOUNTER = 1,
        START_ENCOUNTER = 2,
        PAUSE_ENCOUNTER = 3,
    }

    public class ZoneEncounterEventData : IFusionSerializable
    {
        public GameObject zoneEncounter;
        public ZoneEncounterEventType type;

        public void Serialize(FusionWriter writer)
        {
            writer.Write(zoneEncounter);
            writer.Write((byte)type);
        }

        public void Deserialize(FusionReader reader)
        {
            zoneEncounter = reader.ReadGameObject();
            type = (ZoneEncounterEventType)reader.ReadByte();
        }

        public static ZoneEncounterEventData Create(MobileEncounter zoneEncounter, ZoneEncounterEventType type)
        {
            return new ZoneEncounterEventData()
            {
                zoneEncounter = zoneEncounter.gameObject,
                type = type,
            };
        }
    }

    [Net.DelayWhileTargetLoading]
    public class ZoneEncounterEventMessage : FusionMessageHandler
    {
        public override byte? Tag => NativeMessageTag.ZoneEncounterEvent;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
        {
            using FusionReader reader = FusionReader.Create(bytes);
            var data = reader.ReadFusionSerializable<ZoneEncounterEventData>();
            // We ONLY handle this if we are a client!
            if (!NetworkInfo.IsServer && data.zoneEncounter != null)
            {
                var encounter = data.zoneEncounter.GetComponent<MobileEncounter>();

                ZoneEncounterPatches.IgnorePatches = true;

                if (encounter != null)
                {
                    switch (data.type)
                    {
                        default:
                        case ZoneEncounterEventType.UNKNOWN:
                            break;
                        case ZoneEncounterEventType.COMPLETE_ENCOUNTER:
                            encounter.CompleteEncounter();
                            break;
                        case ZoneEncounterEventType.START_ENCOUNTER:
                            encounter.StartEncounter();
                            break;
                        case ZoneEncounterEventType.PAUSE_ENCOUNTER:
                            encounter.PauseEncounter();
                            break;
                    }
                }

                ZoneEncounterPatches.IgnorePatches = false;
            }
        }
    }
}
