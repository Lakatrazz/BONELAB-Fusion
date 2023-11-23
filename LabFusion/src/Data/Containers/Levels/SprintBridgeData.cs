﻿using LabFusion.Senders;
using LabFusion.Utilities;
using SLZ.Bonelab;
using UnityEngine;

namespace LabFusion.Data
{
    public class SprintBridgeData : LevelDataHandler
    {
        public override string LevelTitle => "07 - Sprint Bridge 04";

        public static GameControl_SprintBridge04 GameController;

        protected override void MainSceneInitialized()
        {
            GameController = GameObject.FindObjectOfType<GameControl_SprintBridge04>();

            if (GameController != null)
            {
                var copter = GameObject.Find("TrashCopter");

                if (copter != null)
                {
                    PropSender.SendPropCreation(copter);
                }
                else
                {
#if DEBUG
                    FusionLogger.Warn("Failed to find TrashCopter in Sprint Bridge!");
#endif
                }
            }
        }
    }
}
