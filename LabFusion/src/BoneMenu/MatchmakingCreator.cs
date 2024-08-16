using BoneLib.BoneMenu;

using LabFusion.Downloading.ModIO;
using LabFusion.Marrow;
using LabFusion.Utilities;

using UnityEngine;

namespace LabFusion.BoneMenu;

public static class MatchmakingCreator
{
    public static event Action<Page> OnFillMatchmakingPage;

    private static Page _matchmakingPage = null;
    public static Page MatchmakingPage => _matchmakingPage;

    public static void CreateMatchmakingPage(Page page)
    {
        _matchmakingPage = page.CreatePage("Matchmaking", Color.red);

        FillPage(_matchmakingPage);
    }

    public static void RecreatePage()
    {
        if (_matchmakingPage == null)
        {
            return;
        }

        _matchmakingPage.RemoveAll();

        FillPage(_matchmakingPage);
    }

    private static void FillPage(Page page)
    {
        // Validate the Fusion Content before allowing the user to use lobbies
        var contentValidation = FusionPalletReferences.ValidateContentPallet();
        if (contentValidation == FusionPalletReferences.PalletStatus.MISSING)
        {
            page.CreateFunction("You are missing the Fusion Content mod from mod.io!", Color.yellow, null);
            page.CreateFunction("This is required for Fusion to function!", Color.yellow, null);

            if (!PlatformHelper.IsAndroid)
            {
                page.CreateFunction("Click to Open the Mod Page on Desktop", Color.white, () =>
                {
                    Application.OpenURL(ModReferences.FusionContentURL);
                });
            }

            return;
        }

        if (contentValidation == FusionPalletReferences.PalletStatus.OUTDATED)
        {
            page.CreateFunction("Your Fusion Content mod is outdated!", Color.yellow, null);
            page.CreateFunction("Please update it for Fusion to function!", Color.yellow, null);
            return;
        }

        // Finally, let other parts of the mod add to the page
        OnFillMatchmakingPage?.Invoke(page);
    }
}