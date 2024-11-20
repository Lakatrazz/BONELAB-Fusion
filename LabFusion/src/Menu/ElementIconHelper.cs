using Il2CppSLZ.Marrow.Warehouse;

using LabFusion.Downloading.ModIO;
using LabFusion.Marrow;
using LabFusion.Marrow.Proxies;

namespace LabFusion.Menu;

public static class ElementIconHelper
{
    public const float WideAspectRatio = 16f / 9f;

    public static void SetProfileIcon(PlayerElement element, string avatarTitle, int modId = -1)
    {
        element.PlayerIconFitter.aspectRatio = 1f;

        var avatarIcon = MenuResources.GetAvatarIcon(avatarTitle);

        if (avatarIcon == null)
        {
            avatarIcon = MenuResources.GetAvatarIcon(MenuResources.ModsIconTitle);
        }

        element.PlayerIcon.texture = avatarIcon;

        if (modId != -1)
        {
            ModIOThumbnailDownloader.GetThumbnail(modId, (texture) =>
            {
                element.PlayerIcon.texture = texture;
                element.PlayerIconFitter.aspectRatio = WideAspectRatio;
            });
        }
    }

    public static void SetProfileResultIcon(PlayerResultElement element, string avatarTitle, int modId = -1)
    {
        element.PlayerIconFitter.aspectRatio = 1f;

        var avatarIcon = MenuResources.GetAvatarIcon(avatarTitle);

        if (avatarIcon == null)
        {
            avatarIcon = MenuResources.GetAvatarIcon(MenuResources.ModsIconTitle);
        }

        element.PlayerIcon.texture = avatarIcon;

        if (modId != -1)
        {
            ModIOThumbnailDownloader.GetThumbnail(modId, (texture) =>
            {
                element.PlayerIcon.texture = texture;
                element.PlayerIconFitter.aspectRatio = WideAspectRatio;
            });
        }
    }

    public static void SetLevelIcon(LobbyElement element, string levelTitle, int modId = -1)
    {
        var levelIcon = MenuResources.GetLevelIcon(levelTitle);

        if (levelIcon == null)
        {
            levelIcon = MenuResources.GetLevelIcon(MenuResources.ModsIconTitle);
        }

        element.LevelIcon.texture = levelIcon;

        if (modId != -1)
        {
            ModIOThumbnailDownloader.GetThumbnail(modId, (texture) =>
            {
                element.LevelIcon.texture = texture;
            });
        }
    }

    public static void SetLevelResultIcon(LobbyResultElement element, string levelTitle, int modId = -1)
    {
        var levelIcon = MenuResources.GetLevelIcon(levelTitle);

        if (levelIcon == null)
        {
            levelIcon = MenuResources.GetLevelIcon(MenuResources.ModsIconTitle);
        }

        element.LevelIcon.texture = levelIcon;

        if (modId != -1)
        {
            ModIOThumbnailDownloader.GetThumbnail(modId, (texture) =>
            {
                element.LevelIcon.texture = texture;
            });
        }
    }
}