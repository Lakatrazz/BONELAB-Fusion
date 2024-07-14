using LabFusion.Senders;
using LabFusion.Utilities;

using Il2CppSLZ.Bonelab;

using UnityEngine;
using Il2CppSLZ.Marrow.Interaction;

namespace LabFusion.Data
{
    public class SprintBridgeData : LevelDataHandler
    {
        public override string LevelTitle => "07 - Sprint Bridge 04";

        public static GameControl_SprintBridge04 GameController;

        protected override void MainSceneInitialized()
        {
            GameController = GameObject.FindObjectOfType<GameControl_SprintBridge04>();

            if (GameController == null)
            {
                return;
            }
        }
    }
}
