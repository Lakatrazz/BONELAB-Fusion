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
    public class HoloChamberBDMData : DMLevelDataHandler
    {
        public override string LevelTitle => "HoloChamber";

        protected override Vector3[] DeathmatchSpawnPoints => new Vector3[6] {
    new Vector3(18.5182f, 0.0371f, 3.7177f),
    new Vector3(18.5824f, 0.0371f, 40.6872f),
    new Vector3(0.0182f, 0.0374f, 0.8058f),
    new Vector3(-18.5055f, 0.0371f, 40.3152f),
    new Vector3(-18.5734f, 0.0371f, 3.203f),
    new Vector3(-0.14f, 0.0371f, 21.9039f),
        };
    }
}
