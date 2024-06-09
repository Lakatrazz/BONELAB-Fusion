using LabFusion.Data;
using LabFusion.Patching;
using LabFusion.Exceptions;

using UnityEngine;
using Il2CppSLZ.Bonelab;

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
