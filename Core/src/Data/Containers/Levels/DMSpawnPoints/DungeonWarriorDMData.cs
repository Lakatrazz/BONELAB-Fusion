using UnityEngine;

namespace LabFusion.Data
{
    public class DungeonWarriorDMData : DMLevelDataHandler
    {
        public override string LevelTitle => "Dungeon Warrior";

        protected override Vector3[] DeathmatchSpawnPoints => new Vector3[7] {
    new Vector3(54.2045f, -7.5652f, -65.2812f),
    new Vector3(62.5462f, 9.3181f, -77.1614f),
    new Vector3(49.9896f, 11.2435f, -75.2906f),
    new Vector3(62.4793f, 3.2703f, -75.1372f),
    new Vector3(58.0072f, 19.0727f, -71.7206f),
    new Vector3(54.3207f, -7.8262f, -73.7793f),
    new Vector3(58.6233f, 21.2966f, -74.2617f),
        };
    }
}
