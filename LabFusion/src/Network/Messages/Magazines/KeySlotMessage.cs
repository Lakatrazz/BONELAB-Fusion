using LabFusion.Network.Serialization;
using LabFusion.Patching;
using LabFusion.Entities;
using LabFusion.Extensions;

using Il2CppSLZ.Interaction;
using Il2CppSLZ.Marrow;

using UnityEngine;

namespace LabFusion.Network;

public enum KeySlotType
{
    UNKNOWN = 0,
    INSERT_STATIC = 1,
    INSERT_PROP = 2,
}

public class KeySlotData : INetSerializable
{
    public const int Size = sizeof(byte) * 2 + sizeof(ushort);

    public byte smallId;
    public KeySlotType type;
    public ushort keyId;

    // Static receiver
    public GameObject receiver;

    // Prop receiver
    public ushort? receiverId;
    public byte? receiverIndex;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref smallId);
        serializer.SerializeValue(ref type, Precision.OneByte);
        serializer.SerializeValue(ref keyId);

        serializer.SerializeValue(ref receiver);

        serializer.SerializeValue(ref receiverId);
        serializer.SerializeValue(ref receiverIndex);
    }

    public static KeySlotData Create(byte smallId, KeySlotType type, ushort keyId, GameObject receiver = null, ushort? receiverId = null, byte? receiverIndex = null)
    {
        return new KeySlotData()
        {
            smallId = smallId,
            type = type,
            keyId = keyId,

            receiver = receiver,

            receiverId = receiverId,
            receiverIndex = receiverIndex,
        };
    }
}

[Net.DelayWhileTargetLoading]
public class KeySlotMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.KeySlot;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<KeySlotData>();

        var key = NetworkEntityManager.IdManager.RegisteredEntities.GetEntity(data.keyId);

        if (key == null)
        {
            return;
        }

        var keyExtender = key.GetExtender<KeyExtender>();

        if (keyExtender == null)
        {
            return;
        }

        KeyRecieverPatches.IgnorePatches = true;

        switch (data.type)
        {
            default:
            case KeySlotType.UNKNOWN:
                break;
            case KeySlotType.INSERT_STATIC:
                if (data.receiver == null)
                {
                    break;
                }

                var keyReceiver = data.receiver.GetComponent<KeyReceiver>();

                if (keyReceiver == null)
                {
                    break;
                }

                var host = InteractableHost.Cache.Get(keyExtender.Component.gameObject);

                // Insert the key and detach grips
                host.TryDetach();

                keyReceiver.OnInteractableHostEnter(host);
                break;
            case KeySlotType.INSERT_PROP:
                var entity = NetworkEntityManager.IdManager.RegisteredEntities.GetEntity(data.receiverId.Value);

                if (entity == null)
                {
                    break;
                }

                var keyReceiverExtender = entity.GetExtender<KeyRecieverExtender>();

                if (keyExtender == null)
                {
                    break;
                }

                var propKeyReceiver = keyReceiverExtender.GetComponent(data.receiverIndex.Value);

                if (propKeyReceiver == null)
                {
                    break;
                }

                var propHost = InteractableHost.Cache.Get(keyExtender.Component.gameObject);

                // Insert the key and detach grips
                propHost.TryDetach();

                propKeyReceiver.OnInteractableHostEnter(propHost);
                break;
        }
    }
}