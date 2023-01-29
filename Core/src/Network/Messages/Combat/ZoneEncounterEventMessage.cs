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
using SLZ.Zones;

using UnityEngine;

namespace LabFusion.Network
{
    public enum ZoneEncounterEventType {
        UNKNOWN = 0,
        START_ENCOUNTER = 1,
        PAUSE_ENCOUNTER = 2,
        COMPLETE_ENCOUNTER = 3,
    }

    public class ZoneEncounterEventData : IFusionSerializable, IDisposable
    {
        public ZoneEncounterEventType type;
        public GameObject gameObject;

        public void Serialize(FusionWriter writer)
        {
            writer.Write((byte)type);
            writer.Write(gameObject);
        }

        public void Deserialize(FusionReader reader)
        {
            type = (ZoneEncounterEventType)reader.ReadByte();
            gameObject = reader.ReadGameObject();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public static ZoneEncounterEventData Create(ZoneEncounterEventType type, ZoneEncounter encounter)
        {
            return new ZoneEncounterEventData()
            {
                type = type,
                gameObject = encounter.gameObject,
            };
        }
    }

    [Net.DelayWhileLoading]
    public class ZoneEncounterEventMessage : FusionMessageHandler
    {
        public override byte? Tag => NativeMessageTag.ZoneEncounterEvent;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
        {
            using (FusionReader reader = FusionReader.Create(bytes))
            {
                using (var data = reader.ReadFusionSerializable<ZoneEncounterEventData>())
                {
                    ZoneEncounterPatches.IgnorePatches = true;

                    if (!NetworkInfo.IsServer && data.gameObject) {
                        var zoneEncounter = data.gameObject.GetComponent<ZoneEncounter>();

                        switch (data.type) {
                            case ZoneEncounterEventType.UNKNOWN:
                                break;
                            case ZoneEncounterEventType.START_ENCOUNTER:
                                zoneEncounter.StartEncounter();
                                break;
                            case ZoneEncounterEventType.PAUSE_ENCOUNTER:
                                zoneEncounter.PauseEncounter();
                                break;
                            case ZoneEncounterEventType.COMPLETE_ENCOUNTER:
                                zoneEncounter.CompleteEncounter();
                                break;
                        }
                    }

                    ZoneEncounterPatches.IgnorePatches = false;
                }
            }
        }
    }
}
