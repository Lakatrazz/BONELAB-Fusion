using HarmonyLib;

using Il2CppSLZ.Marrow.Warehouse;

namespace LabFusion.Marrow.Patching;

public static class CrateSpawnerAndroidPatches
{
    public static void PatchAll()
    {
        var original = typeof(CrateSpawner._SpawnSpawnableAsync_d__26).GetMethod(nameof(CrateSpawner._SpawnSpawnableAsync_d__26.MoveNext), AccessTools.all);

        var prefix = new HarmonyMethod(typeof(CrateSpawnerAndroidPatches).GetMethod(nameof(MoveNextPrefix), AccessTools.all));

        FusionMod.Instance.HarmonyInstance.Patch(original, prefix);
    }
    
    public static bool MoveNextPrefix(CrateSpawner._SpawnSpawnableAsync_d__26 __instance)
    {
        var spawner = __instance.__4__this;

        var awaiter = __instance.__u__1;

        var task = awaiter.task;

        bool value = CrateSpawnerPatches.SpawnSpawnableAsyncPrefix(spawner, false, ref task);

        if (!value)
        {
            awaiter.task = task;
        }

        return value;
    }
}
