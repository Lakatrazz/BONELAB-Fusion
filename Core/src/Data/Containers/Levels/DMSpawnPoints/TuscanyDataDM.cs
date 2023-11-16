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
    public class TuscanyDMData : DMLevelDataHandler
    {
        public override string LevelTitle => "Tuscany";

        protected override Vector3[] DeathmatchSpawnPoints => new Vector3[10] {
   new Vector3(17.886f, 6.9674f, 54.4874f),
    new Vector3(-39.3922f, 0.7033f, 7.8235f),
    new Vector3(-1.6841f, 12.3276f, 5.5593f),
    new Vector3(27.6902f, -0.3047f, -1.3649f),
    new Vector3(-2.8871f, 1.4279f, 21.9112f),
    new Vector3(0.0901f, 5.7132f, -0.9069f),
    new Vector3(-6.3986f, 4.3902f, 6.615f),
    new Vector3(-18.9443f, 1.4807f, -6.1819f),
    new Vector3(0.7016f, 1.4772f, -16.6518f),
    new Vector3(14.0029f, 1.8974f, 4.1357f),
        };
    }
}
