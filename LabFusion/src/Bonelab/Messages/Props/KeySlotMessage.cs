using LabFusion.Network.Serialization;
using LabFusion.Bonelab.Patching;
using LabFusion.Entities;
using LabFusion.Bonelab.Extenders;
using LabFusion.Extensions;
using LabFusion.Network;
using LabFusion.SDK.Modules;

using Il2CppSLZ.Interaction;

using Il2CppSLZ.Marrow;

namespace LabFusion.Bonelab.Messages;

public class KeySlotData : INetSerializable
{
    public const int Size = sizeof(ushort) + ComponentPathData.Size;

    public ushort KeyId;
    public ComponentPathData ReceiverData;

    public int? GetSize() => Size;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref KeyId);

        serializer.SerializeValue(ref ReceiverData);
    }
}

[Net.DelayWhileTargetLoading]
public class KeySlotMessage : ModuleMessageHandler
{
    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<KeySlotData>();

        var keyEntity = NetworkEntityManager.IDManager.RegisteredEntities.GetEntity(data.KeyId);

        if (keyEntity == null)
        {
            return;
        }

        var keyExtender = keyEntity.GetExtender<KeyExtender>();

        if (keyExtender == null)
        {
            return;
        }

        var key = keyExtender.Component;

        var host = InteractableHost.Cache.Get(key.gameObject);

        if (host == null)
        {
            return;
        }

        if (data.ReceiverData.TryGetComponent<KeyReceiver, KeyReceiverExtender>(KeyReceiverPatches.HashTable, out var receiver))
        {
            OnFoundKey(host, receiver);
        }
    }

    private static void OnFoundKey(InteractableHost keyHost, KeyReceiver receiver)
    {
        KeyReceiverPatches.IgnorePatches = true;

        keyHost.TryDetach();

        receiver.OnInteractableHostEnter(keyHost);
    }
}