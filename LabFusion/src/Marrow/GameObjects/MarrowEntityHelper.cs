using Il2CppSLZ.Marrow.Interaction;

using LabFusion.Data;
using LabFusion.Network;
using LabFusion.Patching;

namespace LabFusion.Marrow;

public static class MarrowEntityHelper
{
    public class EntityLookupData : IFusionSerializable
    {
        public int hash;
        public int index;

        public void Serialize(FusionWriter writer)
        {
            writer.Write(hash);
            writer.Write(index);
        }

        public void Deserialize(FusionReader reader)
        {
            hash = reader.ReadInt32();
            index = reader.ReadInt32();
        }
    }

    public static MarrowEntity GetEntityFromLookup(EntityLookupData data)
    {
        if (!MarrowEntityPatches.HashToEntities.TryGetValue(data.hash, out var entities))
        {
            return null;
        }

        if (data.index >= entities.Count || data.index < 0)
        {
            return null;
        }

        return entities[data.index];
    }

    public static EntityLookupData GetLookupFromEntity(MarrowEntity entity)
    {
        if (!MarrowEntityPatches.EntityToHash.TryGetValue(entity, out var hash))
        {
            return null;
        }

        var list = MarrowEntityPatches.HashToEntities[hash];
        var index = list.FindIndex((e) => e == entity);

        return new EntityLookupData()
        {
            hash = hash,
            index = index,
        };
    }
}
