using UnityEngine;

using Il2CppSLZ.Bonelab;

using LabFusion.Marrow.Scene;

namespace LabFusion.Bonelab.Scene;

public class TimeTrialEventHandler : LevelEventHandler
{
    public static TimeTrial_GameController GameController { get; set; }

    protected override void OnLevelLoaded()
    {
        GameController = GameObject.FindObjectOfType<TimeTrial_GameController>();
    }

    public static bool IsInTimeTrial => GameController != null;
}
