using SLZ.Zones;

using UnityEngine;

namespace LabFusion.Extensions
{
    public static class ZoneSpawnerExtensions
    {
        public static void InvokeSpawnEvent(this ZoneSpawner spawner, GameObject spawned)
        {
            // This is the compiler generated method for the ZoneSpawner's OnSpawn event
            // Creating an instance with all the variables and calling the method works
            var onSpawn = new ZoneSpawner.__c__DisplayClass76_0
            {
                __4__this = spawner,
                playerObject = SceneZone.PlayerObject
            };

            onSpawn._Spawn_b__0(spawned);
        }
    }
}
