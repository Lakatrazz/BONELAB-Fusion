using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

using SLZ.Bonelab;

using LabFusion.Extensions;

namespace LabFusion.Data {
    public static class GameControllerData {
        public static BaseGameController GameController;

        public static void OnCacheInfo() {
            GameController = GameObject.FindObjectOfType<BaseGameController>();
        }

        public static bool HasGameController => !GameController.IsNOC();
    }
}
