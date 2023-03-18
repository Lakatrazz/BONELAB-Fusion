using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

using SLZ.Bonelab;
using SLZ.Vehicle;

using LabFusion.Extensions;
using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.Syncables;
using LabFusion.Utilities;

namespace LabFusion.Data
{
    public class MagmaGateData : LevelDataHandler
    {
        public static GameControl_MagmaGate GameController;

        protected override void MainSceneInitialized()
        {
            GameController = GameObject.FindObjectOfType<GameControl_MagmaGate>(true);
        }
    }
}
