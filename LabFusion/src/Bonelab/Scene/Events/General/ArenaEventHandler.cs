using UnityEngine;

using Il2CppSLZ.Bonelab;

using LabFusion.Marrow.Scene;

namespace LabFusion.Bonelab.Scene;

public class ArenaEventHandler : LevelEventHandler
{
    public static Arena_GameController GameController { get; set; }
    public static GenGameControl_Display GameControlDisplay { get; set; }
    public static ArenaMenuController MenuController { get; set; }
    public static GeoManager GeoManager { get; set; }
    public static ChallengeSelectMenu[] ChallengeSelections { get; set; }

    protected override void OnLevelLoaded()
    {
        GameController = GameObject.FindObjectOfType<Arena_GameController>();

        if (GameController != null)
        {
            ChallengeSelections = GameObject.FindObjectsOfType<ChallengeSelectMenu>(true);
            GameControlDisplay = GameObject.FindObjectOfType<GenGameControl_Display>(true);
            MenuController = GameObject.FindObjectOfType<ArenaMenuController>(true);
            GeoManager = GameObject.FindObjectOfType<GeoManager>(true);
        }
    }

    public static bool IsInArena => GameController != null;

    public static byte? GetIndex(ChallengeSelectMenu menu)
    {
        for (byte i = 0; i < ChallengeSelections.Length; i++)
        {
            if (ChallengeSelections[i] == menu)
            {
                return i;
            }
        }

        return null;
    }

    public static ChallengeSelectMenu GetMenu(byte index)
    {
        if (ChallengeSelections != null && ChallengeSelections.Length > index)
        {
            return ChallengeSelections[index];
        }

        return null;
    }
}
