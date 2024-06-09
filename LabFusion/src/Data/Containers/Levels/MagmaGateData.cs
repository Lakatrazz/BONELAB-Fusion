using UnityEngine;

using Il2CppSLZ.Bonelab;

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
