using LabFusion.Bonelab.Extenders;
using LabFusion.Entities;
using LabFusion.Network;
using LabFusion.Network.Serialization;
using LabFusion.SDK.Modules;

namespace LabFusion.Bonelab.Messages;

public class RandomObjectData : INetSerializable
{
    public const int Size = sizeof(byte) + sizeof(ushort) * 3;

    public ushort entityId;
    public ushort componentIndex;

    public ushort objectIndex;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref entityId);
        serializer.SerializeValue(ref componentIndex);
        serializer.SerializeValue(ref objectIndex);
    }

    public static RandomObjectData Create(ushort entityId, ushort componentIndex, ushort objectIndex)
    {
        return new RandomObjectData()
        {
            entityId = entityId,
            componentIndex = componentIndex,
            objectIndex = objectIndex,
        };
    }
}

public class RandomObjectMessage : ModuleMessageHandler
{
    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<RandomObjectData>();

        // Right now only syncs RandomObject on individual objects (props, avatars, etc). No scene syncing yet.
        var entity = NetworkEntityManager.IDManager.RegisteredEntities.GetEntity(data.entityId);

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