using LabFusion.Downloading.ModIO;
using LabFusion.Marrow.Proxies;

using UnityEngine.UI;

namespace LabFusion.Menu;

public static class ElementIconHelper
{
    public const float WideAspectRatio = 16f / 9f;

    public static void SetProfileIcon(RawImage rawImage, AspectRatioFitter aspectRatioFitter, string avatarTitle, int modID = -1)
    {
        aspectRatioFitter.aspectRatio = 1f;

        var icon = MenuResources.GetAvatarIcon(avatarTitle);

        if (icon == null)
        {
            icon = MenuResources.GetAvatarIcon(MenuResources.ModsIconTitle);
        }

        rawImage.texture = icon;

        if (modID != -1)
        {
            ModIOThumbnailDownloader.GetThumbnail(modID, (texture) =>
            {
                if (rawImage == null)
                {
                    return;
                }

                rawImage.texture = texture;
                aspectRatioFitter.aspectRatio = WideAspectRatio;
            });
        }
    }

    public static void SetProfileIcon(PlayerElement element, string avatarTitle, int modID = -1) => SetProfileIcon(element.PlayerIcon, element.PlayerIconFitter, avatarTitle, modID);

    public static void SetProfileResultIcon(PlayerResultElement element, string avatarTitle, int modID = -1) => SetProfileIcon(element.PlayerIcon, element.PlayerIconFitter, avatarTitle, modID);

    public static void SetLevelIcon(RawImage rawImage, string levelTitle, int modID = -1)
    {
        var icon = MenuResources.GetLevelIcon(levelTitle);

        if (icon == null)
        {
            icon = MenuResources.GetLevelIcon(MenuResources.ModsIconTitle);
        }

        rawImage.texture = icon;

        if (modID != -1)
        {
            ModIOThumbnailDownloader.GetThumbnail(modID, (texture) =>
            {
                if (rawImage == null)
                {
                    return;
                }

                rawImage.texture = texture;
            });
        }
    }

    public static void SetLevelIcon(LobbyElement element, string levelTitle, int modID = -1) => SetLevelIcon(element.LevelIcon, levelTitle, modID);

    public static void SetLevelResultIcon(LobbyResultElement element, string levelTitle, int modID = -1) => SetLevelIcon(element.LevelIcon, levelTitle, modID);

    public static void SetGamemodeIcon(LobbyElement element, string gamemodeTitle)
    {
        var gamemodeIcon = MenuResources.GetGamemodeIcon(MenuResources.SandboxIconTitle);

        if (!string.IsNullOrWhiteSpace(gamemodeTitle))
        {
            gamemodeIcon = MenuResources.GetGamemodeIcon(gamemodeTitle);
        }

        if (gamemodeIcon == null)
        {
            gamemodeIcon = MenuResources.GetGamemodeIcon(MenuResources.ModsIconTitle);
        }

        element.GamemodeIcon.texture = gamemodeIcon;
    }
}