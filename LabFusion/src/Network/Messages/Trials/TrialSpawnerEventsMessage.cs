using LabFusion.Patching;
using LabFusion.Network.Serialization;
using LabFusion.Data;

namespace LabFusion.Network;

public class TrialSpawnerEventsData : INetSerializable
{
    public ComponentHashData HashData;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref HashData);
    }

    public static TrialSpawnerEventsData Create(ComponentHashData hashData)
    {
        return new TrialSpawnerEventsData()
        {
            HashData = hashData,
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

        var trialSpawnerEvents = Trial_SpawnerEventsPatches.HashTable.GetComponentFromData(data.HashData);

        if (trialSpawnerEvents == null)
        {
            return;
        }

        Trial_SpawnerEventsPatches.IgnorePatches = true;

        trialSpawnerEvents.OnSpawnerDeath();
    }
}
