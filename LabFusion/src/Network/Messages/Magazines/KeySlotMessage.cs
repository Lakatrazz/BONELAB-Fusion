using LabFusion.Data;
using LabFusion.Patching;
using LabFusion.Entities;
using LabFusion.Extensions;

using Il2CppSLZ.Interaction;

using UnityEngine;

namespace LabFusion.Network;

public enum KeySlotType
{
    UNKNOWN = 0,
    INSERT_STATIC = 1,
    INSERT_PROP = 2,
}

public class KeySlotData : IFusionSerializable
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

    public void Serialize(FusionWriter writer)
    {
        writer.Write(smallId);
        writer.Write((byte)type);
        writer.Write(keyId);

        writer.Write(receiver);

        writer.Write(receiverId);
        writer.Write(receiverIndex);
    }

    public void Deserialize(FusionReader reader)
    {
        smallId = reader.ReadByte();
        type = (KeySlotType)reader.ReadByte();
        keyId = reader.ReadUInt16();

        receiver = reader.ReadGameObject();

        receiverId = reader.ReadUInt16Nullable();
        receiverIndex = reader.ReadByteNullable();
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
public class KeySlotMessage : FusionMessageHandler
{
    public override byte? Tag => NativeMessageTag.KeySlot;

    public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
    {
        using FusionReader reader = FusionReader.Create(bytes);
        var data = reader.ReadFusionSerializable<KeySlotData>();

        // Send message to other clients if server
        if (isServerHandled)
        {
            using var message = FusionMessage.Create(Tag.Value, bytes);
            MessageSender.BroadcastMessageExcept(data.smallId, NetworkChannel.Reliable, message, false);

            return;
        }

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

        KeyRecieverPatches.IgnorePatches = false;
    }
}