using LabFusion.Utilities;
using SLZ.Bonelab;
using SLZ.UI;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.Data {
    public static class DescentData {
        public static NooseBonelabIntro Noose;
        public static TutorialElevator Elevator;
        public static GameControl_Descent GameController;
        public static Control_UI_BodyMeasurements BodyMeasurementsUI;

        public static void OnCacheInfo() {
            Noose = GameObject.FindObjectOfType<NooseBonelabIntro>(true);
            Elevator = GameObject.FindObjectOfType<TutorialElevator>(true);
            GameController = GameObject.FindObjectOfType<GameControl_Descent>(true);
            BodyMeasurementsUI = GameObject.FindObjectOfType<Control_UI_BodyMeasurements>(true);
        }
    }
}
