using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

using SLZ.Bonelab;

using LabFusion.Extensions;

namespace LabFusion.Data {
    public static class TimeTrialData {
        public static TimeTrial_GameController GameController;

        public static void OnCacheInfo() {
            GameController = GameObject.FindObjectOfType<TimeTrial_GameController>();
        }

        public static bool IsInTimeTrial => !GameController.IsNOC();
    }
}
