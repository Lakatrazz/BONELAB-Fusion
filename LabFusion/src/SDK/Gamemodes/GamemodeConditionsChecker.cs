using LabFusion.Player;
using LabFusion.Utilities;

namespace LabFusion.SDK.Gamemodes;

public static class GamemodeConditionsChecker
{
    public static void OnInitializeMelon()
    {
        GamemodeManager.OnGamemodeChanged += OnGamemodeChanged;

        MultiplayerHooking.OnPlayerJoin += OnPlayerCountChanged;
        MultiplayerHooking.OnPlayerLeave += OnPlayerCountChanged;

        MultiplayerHooking.OnMainSceneInitialized += AutoCheckConditions;
    }

    private static void OnGamemodeChanged(Gamemode gamemode)
    {
        AutoCheckConditions();
    }

    private static void OnPlayerCountChanged(PlayerId player)
    {
        AutoCheckConditions();
    }

    private static void AutoCheckConditions()
    {
        if (GamemodeManager.ActiveGamemode == null)
        {
            return;
        }

        if (!GamemodeManager.ActiveGamemode.ManualReady)
        {
            GamemodeManager.ValidateReadyConditions();
        }
    }
}
