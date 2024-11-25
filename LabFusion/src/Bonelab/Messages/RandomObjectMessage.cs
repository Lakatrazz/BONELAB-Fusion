using LabFusion.Bonelab.Extenders;
using LabFusion.Data;
using LabFusion.Entities;
using LabFusion.Network;

using LabFusion.SDK.Modules;

namespace LabFusion.Bonelab;

public class RandomObjectData : IFusionSerializable
{
    public const int Size = sizeof(byte) + sizeof(ushort) * 3;

    public byte smallId;

    public ushort entityId;
    public ushort componentIndex;

    public ushort objectIndex;

    public void Serialize(FusionWriter writer)
    {
        writer.Write(smallId);
        writer.Write(entityId);
        writer.Write(componentIndex);
        writer.Write(objectIndex);
    }

    public void Deserialize(FusionReader reader)
    {
        smallId = reader.ReadByte();
        entityId = reader.ReadUInt16();
        componentIndex = reader.ReadUInt16();
        objectIndex = reader.ReadUInt16();
    }

    public static RandomObjectData Create(byte smallId, ushort entityId, ushort componentIndex, ushort objectIndex)
    {
        return new RandomObjectData()
        {
            smallId = smallId,
            entityId = entityId,
            componentIndex = componentIndex,
            objectIndex = objectIndex,
        };
    }
}

public class RandomObjectMessage : ModuleMessageHandler
{
    public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
    {
        using var reader = FusionReader.Create(bytes);

        var data = reader.ReadFusionSerializable<RandomObjectData>();

        // Send message to other clients if server
        if (isServerHandled)
        {
            using var message = FusionMessage.ModuleCreate<RandomObjectMessage>(bytes);
            MessageSender.BroadcastMessageExcept(data.smallId, NetworkChannel.Reliable, message, false);
            return;
        }

        // Right now only syncs RandomObject on individual objects (props, avatars, etc). No scene syncing yet.
        var entity = NetworkEntityManager.IdManager.RegisteredEntities.GetEntity(data.entityId);

        if (entity == null)
        {
            return;
        }

        var extender = entity.GetExtender<RandomObjectExtender>();

        if (extender == null)
        {
            return;
        }

        var randomObject = extender.GetComponent(data.componentIndex);

        if (randomObject == null)
        {
            return;
        }

        // Invalid index
        if (data.objectIndex < 0 || data.objectIndex >= randomObject.Objects.Count)
        {
            return;
        }

        // Disable all active objects
        foreach (var obj in randomObject.Objects)
        {
            obj.SetActive(false);
        }

        // Enable the target
        randomObject.Objects[data.objectIndex].SetActive(true);
    }
}