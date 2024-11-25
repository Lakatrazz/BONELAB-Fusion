using LabFusion.Network;
using LabFusion.Scene;
using LabFusion.Utilities;

namespace LabFusion.SDK.Gamemodes;

public static class GamemodeHelper
{
    public static void StartGamemodeServer(Gamemode gamemode)
    {
        if (NetworkInfo.HasServer)
        {
            NetworkHelper.Disconnect();
        }

        NetworkHelper.StartServer();

        DelayUtilities.Delay(() =>
        {
            FusionSceneManager.HookOnLevelLoad(() =>
            {
                GamemodeManager.SelectGamemode(gamemode);
            });
        }, 10);
    }
}
