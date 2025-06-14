using Il2CppSLZ.Marrow.Interaction;

using LabFusion.Data;
using LabFusion.Marrow.Patching;

namespace LabFusion.Marrow;

public static class MarrowEntityHelper
{
    public static MarrowEntity GetEntityFromData(ComponentHashData data)
    {
        return MarrowEntityPatches.HashTable.GetComponentFromData(data);
    }

    public static ComponentHashData GetDataFromEntity(MarrowEntity entity)
    {
        return MarrowEntityPatches.HashTable.GetDataFromComponent(entity);
    }

    public static bool IsHashed(MarrowEntity entity) => MarrowEntityPatches.HashTable.IsHashed(entity);
}
