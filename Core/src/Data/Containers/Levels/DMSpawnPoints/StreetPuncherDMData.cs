using LabFusion.MarrowIntegration;
using LabFusion.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.Data {
    public class StreetPuncherDMData : DMLevelDataHandler {
        public override string LevelTitle => "06 - Street Puncher";
        protected override Vector3[] DeathmatchSpawnPoints => new Vector3[12] {
    new Vector3(-1.4924f, 11.7874f, 94.0635f),
    new Vector3(3.5141f, 13.4904f, 101.4765f),
    new Vector3(9.0983f, 17.7881f, 134.1948f),
    new Vector3(15.2911f, 11.7874f, 103.3306f),
    new Vector3(0.5954f, 7.7874f, 90.02f),
    new Vector3(20.7147f, 1.95f, 93.0073f),
    new Vector3(9.5308f, 1.7873f, 82.1337f),
    new Vector3(3.2431f, 1.7874f, 135.6179f),
    new Vector3(-4.844f, 2.0374f, 111.886f),
    new Vector3(13.5487f, 9.7874f, 122.6553f),
    new Vector3(6.6608f, 11.3569f, 110.1438f),
    new Vector3(-5.1882f, 17.7874f, 134.6813f),
        };
    }
}
