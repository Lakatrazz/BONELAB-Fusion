using SLZ.AI;
using SLZ.Zones;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Extensions {
    public static class ZoneSpawnerExtensions {
        public static void InsertNPC(this ZoneSpawner spawner, AIBrain brain) {
            // Call spawn event
            spawner.onSpawnNPCDelegate?.Invoke(spawner, brain, spawner.currEnemyProfile, false);

            // Hook brain events
            brain.onDeathDelegate += (Action)(() => { spawner.OnDeath(); });
            brain.onResurrectDelegate += (Action)(() => { spawner.OnResurrect(); });

            // Update enemy config
            if (brain.behaviour != null && spawner.currEnemyProfile != null && spawner.currEnemyProfile.baseConfig != null)
                brain.behaviour.SetBaseConfig(spawner.currEnemyProfile.baseConfig);
        }
    }
}
