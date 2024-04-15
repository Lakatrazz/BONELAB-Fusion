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
using LabFusion.Exceptions;

using SLZ;
using SLZ.Interaction;
using SLZ.Props.Weapons;

using UnityEngine;
using SLZ.Bonelab;

namespace LabFusion.Network
{
    public class TrialSpawnerEventsData : IFusionSerializable
    {
        public GameObject trialSpawnerEvents;

        public void Serialize(FusionWriter writer)
        {
            writer.Write(trialSpawnerEvents);
        }

        public void Deserialize(FusionReader reader)
        {
            trialSpawnerEvents = reader.ReadGameObject();
        }

        public static TrialSpawnerEventsData Create(Trial_SpawnerEvents trialSpawnerEvents)
        {
            return new TrialSpawnerEventsData()
            {
                trialSpawnerEvents = trialSpawnerEvents.gameObject,
            };
        }
    }

    [Net.DelayWhileTargetLoading]
    public class TrialSpawnerEventsMessage : FusionMessageHandler
    {
        public override byte? Tag => NativeMessageTag.TrialSpawnerEvents;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
        {
            using FusionReader reader = FusionReader.Create(bytes);
            var data = reader.ReadFusionSerializable<TrialSpawnerEventsData>();
            var go = data.trialSpawnerEvents;

            // We ONLY handle this for clients, this message should only ever be sent by the server!
            if (!NetworkInfo.IsServer && go)
            {
                var events = go.GetComponent<Trial_SpawnerEvents>();

                Trial_SpawnerEventsPatches.IgnorePatches = true;
                events.OnSpawnerDeath();
                Trial_SpawnerEventsPatches.IgnorePatches = false;
            }
            else
                throw new ExpectedClientException();
        }
    }
}
