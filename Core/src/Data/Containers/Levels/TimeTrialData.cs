using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

using SLZ.Bonelab;

using LabFusion.Extensions;

namespace LabFusion.Data {
    public class TimeTrialData : LevelDataHandler
    {
        // This should always apply to all levels.
        public override string LevelTitle => null;

        public static TimeTrial_GameController GameController;

        protected override void MainSceneInitialized() {
            GameController = GameObject.FindObjectOfType<TimeTrial_GameController>();
        }

        public static bool IsInTimeTrial => !GameController.IsNOC();
    }
}
