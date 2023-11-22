using System;
using SLZ.AI;
using SLZ.Zones;

namespace LabFusion.Extensions
{
    public static class ZoneSpawnerExtensions
    {
        public static void InsertNPC(this ZoneSpawner spawner, AIBrain brain)
        {
            // Call spawn event
            spawner.onSpawnNPCDelegate?.Invoke(spawner, brain, spawner.currEnemyProfile, false);

            // Hook brain events
            brain.onDeathDelegate += (Action)(() => { spawner.OnDeath(); });
            brain.onResurrectDelegate += (Action)(() => { spawner.OnResurrect(); });
        }
    }
}
