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

using LabFusion.Exceptions;
using UnityEngine;
using SLZ.Zones;

namespace LabFusion.Network
{
    public enum ZoneEncounterEventType {
        UNKNOWN = 0,
        COMPLETE_ENCOUNTER = 1,
        START_ENCOUNTER = 2,
        PAUSE_ENCOUNTER = 3,
    }

    public class ZoneEncounterEventData : IFusionSerializable, IDisposable
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

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public static ZoneEncounterEventData Create(ZoneEncounter zoneEncounter, ZoneEncounterEventType type)
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
            using (FusionReader reader = FusionReader.Create(bytes))
            {
                using (var data = reader.ReadFusionSerializable<ZoneEncounterEventData>())
                {
                    // We ONLY handle this if we are a client!
                    if (!NetworkInfo.IsServer && data.zoneEncounter != null) {
                        var encounter = data.zoneEncounter.GetComponent<ZoneEncounter>();

                        ZoneEncounterPatches.IgnorePatches = true;

                        if (encounter != null) {
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
    }
}
