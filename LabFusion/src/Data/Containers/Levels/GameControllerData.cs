using UnityEngine;

using Il2CppSLZ.Bonelab;

using LabFusion.Extensions;

namespace LabFusion.Data
{
    public class GameControllerData : LevelDataHandler
    {
        // This should always apply to all levels.
        public override string LevelTitle => null;

        public static BaseGameController GameController;

        protected override void MainSceneInitialized()
        {
            GameController = GameObject.FindObjectOfType<BaseGameController>();
        }

        public static bool HasGameController => !GameController.IsNOC();
    }
}
