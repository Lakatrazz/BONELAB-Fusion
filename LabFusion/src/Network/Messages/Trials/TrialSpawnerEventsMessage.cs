using LabFusion.Patching;

using UnityEngine;

using Il2CppSLZ.Bonelab;

using LabFusion.Network.Serialization;

namespace LabFusion.Network;

public class TrialSpawnerEventsData : INetSerializable
{
    public GameObject trialSpawnerEvents;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref trialSpawnerEvents);
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
public class TrialSpawnerEventsMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.TrialSpawnerEvents;

    public override ExpectedReceiverType ExpectedReceiver => ExpectedReceiverType.ClientsOnly;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<TrialSpawnerEventsData>();
        var go = data.trialSpawnerEvents;

        var events = go.GetComponent<Trial_SpawnerEvents>();

        Trial_SpawnerEventsPatches.IgnorePatches = true;

        events.OnSpawnerDeath();

        Trial_SpawnerEventsPatches.IgnorePatches = false;
    }
}
