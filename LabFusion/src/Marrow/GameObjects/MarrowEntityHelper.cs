using Il2CppSLZ.Marrow.Interaction;

using LabFusion.Data;
using LabFusion.Patching;

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
}
