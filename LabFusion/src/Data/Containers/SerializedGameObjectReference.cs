using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.Entities;
using LabFusion.Utilities;

using UnityEngine;

using Il2CppSLZ.Marrow.Interaction;

namespace LabFusion.Data;

public class SerializedGameObjectReference : IFusionSerializable
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

    public void Serialize(FusionWriter writer)
    {
        if (gameObject == null)
        {
            writer.Write((byte)ReferenceType.NULL);
            return;
        }

        // Check if there is an entity, and write it
        var marrowBody = MarrowBody.Cache.Get(gameObject);
        if (marrowBody != null && MarrowBodyExtender.Cache.TryGet(marrowBody, out var entity))
        {
            var extender = entity.GetExtender<MarrowBodyExtender>();

            writer.Write((byte)ReferenceType.NETWORK_ENTITY);
            writer.Write(entity.Id);
            writer.Write(extender.GetIndex(marrowBody).Value);
        }
        // Write the full path to the object
        else
        {
            writer.Write((byte)ReferenceType.FULL_PATH);
            writer.Write(gameObject);
        }
    }

    public void Deserialize(FusionReader reader)
    {
        var type = (ReferenceType)reader.ReadByte();

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
                gameObject = reader.ReadGameObject();
                break;
        }
    }
}