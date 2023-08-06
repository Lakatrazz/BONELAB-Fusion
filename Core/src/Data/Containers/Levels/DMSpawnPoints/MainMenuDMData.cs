using LabFusion.MarrowIntegration;
using LabFusion.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.Data {
    public class MainMenuDMData : DMLevelDataHandler {
        public override string LevelTitle => "00 - Main Menu";

        protected override Vector3[] DeathmatchSpawnPoints => new Vector3[4] {
    new Vector3(29.8048f, -1.1377f, 0.8369f),
    new Vector3(26.6286f, -1.1377f, 0.9376f),
    new Vector3(29.5723f, -1.1377f, -1.0273f),
    new Vector3(26.4974f, -1.1377f, -1.1975f),
        };
    }
}
