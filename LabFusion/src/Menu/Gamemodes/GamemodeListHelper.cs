using LabFusion.Marrow.Proxies;
using LabFusion.SDK.Gamemodes;

namespace LabFusion.Menu.Gamemodes;

public static class GamemodeListHelper
{
    public static void RefreshGamemodeList(PageElement pageElement, Action<Gamemode> onGamemodeSelected = null)
    {
        pageElement.Clear();

        List<GamemodeResultElement> resultElements = new();

        foreach (var gamemode in GamemodeManager.Gamemodes)
        {
            var gamemodeResult = pageElement.AddElement<GamemodeResultElement>(gamemode.Title);

            resultElements.Add(gamemodeResult);

            gamemodeResult.GetReferences();

            // Title
            gamemodeResult.GamemodeNameText.text = gamemode.Title;

            // Logo
            var logo = gamemode.Logo ? gamemode.Logo : MenuResources.GetGamemodeIcon(MenuResources.ModsIconTitle);

            gamemodeResult.GamemodeIcon.texture = logo;

            // Selection logic
            gamemodeResult.OnPressed += () =>
            {
                foreach (var other in resultElements)
                {
                    other.Highlight(false);
                }

                gamemodeResult.Highlight(true);

                onGamemodeSelected?.Invoke(gamemode);
            };
        }

        onGamemodeSelected?.Invoke(null);
    }
}
