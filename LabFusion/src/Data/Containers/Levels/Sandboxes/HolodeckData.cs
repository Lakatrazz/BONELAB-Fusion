using Il2CppSLZ.Bonelab;

using UnityEngine;

namespace LabFusion.Data
{
    public class HolodeckData : LevelDataHandler
    {
        public override string LevelTitle => "HoloChamber";

        public static GameControl_Holodeck GameController;
        protected override void MainSceneInitialized()
        {
            GameController = GameObject.FindObjectOfType<GameControl_Holodeck>(true);
        }
    }
}
