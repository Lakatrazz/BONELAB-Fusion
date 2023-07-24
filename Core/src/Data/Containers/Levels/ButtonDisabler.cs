using LabFusion.Data;
using LabFusion.Preferences;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LabFusion.Core.src.Data.Containers.Levels
{
    internal class ButtonDisabler : LevelDataHandler
    {
        protected override void MainSceneInitialized()
        {
            LevelSwitchingButtonDisabler();
        }
        public static void LevelSwitchingButtonDisabler()
        {
            //Get all objects in scene
            var objectsWithKeyword = Transform.FindObjectsOfType<Transform>(true);
            //Loop until it finds a object with a name
            foreach (Transform obj in objectsWithKeyword)
            {
                if (obj.name.Contains("FLOORS") || obj.name.Contains("LoadButtons") || obj.name.Contains("prop_bigButton") || obj.name.Contains("INTERACTION"))
                {
                    for (int i = 0; i < obj.childCount; i++)
                    {
                        //Disable the button omponent
                        Transform child = obj.GetChild(i);
                        SLZ.Interaction.ButtonToggle ButtonToggle = child.GetComponent<SLZ.Interaction.ButtonToggle>();
                        if (ButtonToggle != null && !child.name.Contains("prop_bigButton_NEXTLEVEL") && FusionPreferences.LocalServerSettings.LevelSwitchingButtonsEnabled.GetValue() == false)
                        {
                            ButtonToggle.enabled = false;
                        }
                        else if (ButtonToggle != null && !child.name.Contains("prop_bigButton_NEXTLEVEL") && FusionPreferences.LocalServerSettings.LevelSwitchingButtonsEnabled.GetValue() == true)
                        {
                            ButtonToggle.enabled = true;
                        }
                    }
                }
            }
        }
    }
}
