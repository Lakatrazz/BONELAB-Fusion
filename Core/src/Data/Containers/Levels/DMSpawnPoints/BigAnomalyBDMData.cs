using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.Data {
    public class BigAnomalyBDMData : DMLevelDataHandler {
        public override string LevelTitle => "12 - Big Anomaly B";

        protected override Vector3[] DeathmatchSpawnPoints => new Vector3[6] {
    new Vector3(-29.0873f, 30.0374f, 53.9518f),
    new Vector3(-27.3884f, 25.0374f, 84.7466f),
    new Vector3(-18.74f, 25.0374f, 108.7661f),
    new Vector3(-15.6366f, 25.0373f, 124.4391f),
    new Vector3(-25.2926f, 25.0373f, 124.2993f),
    new Vector3(-27.2077f, 30.0374f, 83.2289f),
        };
    }
}
