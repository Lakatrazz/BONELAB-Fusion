using LabFusion.Extensions;
using SLZ.Bonelab;
using UnityEngine;

namespace LabFusion.Data
{
    public class TimeTrialData : LevelDataHandler
    {
        // This should always apply to all levels.
        public override string LevelTitle => null;

        public static TimeTrial_GameController GameController;

        protected override void MainSceneInitialized()
        {
            GameController = GameObject.FindObjectOfType<TimeTrial_GameController>();
        }

        public static bool IsInTimeTrial => !GameController.IsNOC();
    }
}
