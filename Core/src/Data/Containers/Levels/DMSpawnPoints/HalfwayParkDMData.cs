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
    public class HalfwayParkDMData : DMLevelDataHandler
    {
        public override string LevelTitle => "Halfway Park";

        protected override Vector3[] DeathmatchSpawnPoints => new Vector3[5] {
            new Vector3(-85.0378f, 15.1181f, 85.4412f),
            new Vector3(9.0977f, 9.117f, 1.0171f),
            new Vector3(-49.9463f, 11.1174f, 85.6447f),
            new Vector3(-36.0197f, -5.8826f, 54.2049f),
            new Vector3(-79.8466f, -1.8826f, -9.0016f),
        };
    }
}
