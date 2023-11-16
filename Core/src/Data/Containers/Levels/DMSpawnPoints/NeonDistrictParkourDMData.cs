using LabFusion.MarrowIntegration;
using LabFusion.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.Data
{
    public class NeonDistrictParkourDMData : DMLevelDataHandler
    {
        public override string LevelTitle => "Neon District Parkour";

        protected override Vector3[] DeathmatchSpawnPoints => new Vector3[12] {
    new Vector3(14.1986f, 0.9377f, -7.2248f),
    new Vector3(4.0275f, 4.3721f, -3.6219f),
    new Vector3(17.0486f, 3.0206f, -14.0138f),
    new Vector3(13.5923f, 1.1912f, -13.8777f),
    new Vector3(18.5329f, 4.9916f, -24.4213f),
    new Vector3(8.154f, 3.3383f, -30.4304f),
    new Vector3(-3.1803f, 8.2037f, -31.3292f),
    new Vector3(-13.4639f, 3.3676f, -29.011f),
    new Vector3(-9.9654f, 1.3233f, -21.4282f),
    new Vector3(-9.157f, 1.3293f, -9.5022f),
    new Vector3(-2.9644f, 1.2189f, -16.9085f),
    new Vector3(1.1659f, 1.2677f, -7.5986f),
        };
    }
}
