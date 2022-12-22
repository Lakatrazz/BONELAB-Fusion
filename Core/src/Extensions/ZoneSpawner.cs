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
            // Hook brain events
            brain.onDeathDelegate += (Action)(() => { spawner.OnDeathDelegate.Invoke(); });

            // Update enemy config
            if (brain.behaviour != null && spawner.currEnemyProfile.baseConfig != null)
                brain.behaviour.SetBaseConfig(spawner.currEnemyProfile.baseConfig);
        }
    }
}
