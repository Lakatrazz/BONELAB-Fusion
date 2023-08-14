using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

using SLZ.Bonelab;

namespace LabFusion.Data
{
    public class MagmaGateData : LevelDataHandler
    {
        public override string LevelTitle => "08 - Magma Gate";

        public static GameControl_MagmaGate GameController;

        protected override void MainSceneInitialized()
        {
            GameController = GameObject.FindObjectOfType<GameControl_MagmaGate>(true);
        }
    }
}
