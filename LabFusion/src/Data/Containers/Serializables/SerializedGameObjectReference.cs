using LabFusion.Entities;
using LabFusion.Network.Serialization;

using UnityEngine;

using Il2CppSLZ.Marrow.Interaction;

namespace LabFusion.Data;

public class SerializedGameObjectReference : INetSerializable
{
    private enum ReferenceType
    {
        UNKNOWN = 0,
        FULL_PATH = 1,
        NETWORK_ENTITY = 2,
        NULL = 3,
    }

    // Base gameobject reference
    public GameObject gameObject;

    public SerializedGameObjectReference() { }

    public SerializedGameObjectReference(GameObject go)
    {
        gameObject = go;
    }

    public void Serialize(INetSerializer serializer)
    {
        if (serializer is NetWriter writer)
        {
            Serialize(writer);
        }
        else if (serializer is NetReader reader)
        {
            Deserialize(reader);
        }
    }

    public void Serialize(NetWriter writer)
    {
        if (gameObject == null)
        {
            writer.Write(ReferenceType.NULL, Precision.OneByte);
            return;
        }

        // Check if there is an entity, and write it
        var marrowBody = MarrowBody.Cache.Get(gameObject);
        if (marrowBody != null && MarrowBodyExtender.Cache.TryGet(marrowBody, out var entity))
        {
            var extender = entity.GetExtender<MarrowBodyExtender>();

            writer.Write(ReferenceType.NETWORK_ENTITY, Precision.OneByte);
            writer.Write(entity.Id);
            writer.Write(extender.GetIndex(marrowBody).Value);
        }
        // Write the full path to the object
        else
        {
            writer.Write(ReferenceType.FULL_PATH, Precision.OneByte);
            writer.SerializeValue(ref gameObject);
        }
    }

    public void Deserialize(NetReader reader)
    {
        var type = reader.ReadEnum<ReferenceType>(Precision.OneByte);

        switch (type)
        {
            default:
            case ReferenceType.UNKNOWN:
            case ReferenceType.NULL:
                // Do nothing for null
                break;
            case ReferenceType.NETWORK_ENTITY:
                var id = reader.ReadUInt16();
                var index = reader.ReadUInt16();

                var entity = NetworkEntityManager.IdManager.RegisteredEntities.GetEntity(id);

                if (entity == null)
                {
                    break;
                }

                var extender = entity.GetExtender<MarrowBodyExtender>();

                if (extender == null)
                {
                    break;
                }

                var body = extender.GetComponent(index);

                if (body != null)
                {
                    gameObject = body.gameObject;
                }
                break;
            case ReferenceType.FULL_PATH:
                reader.SerializeValue(ref gameObject);
                break;
        }
    }
}