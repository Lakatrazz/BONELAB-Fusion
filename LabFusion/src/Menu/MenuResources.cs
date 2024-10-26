using UnityEngine;
using UnityEngine.UI;

namespace LabFusion.Menu;

public static class MenuResources
{
    public static Sprite MenuIconSprite { get; private set; } = null;

    public static Dictionary<string, Texture> LevelIconLookup { get; private set; } = null;
    public static Dictionary<string, Texture> AvatarIconLookup { get; private set; } = null;

    public const string ModsIconTitle = "Mods";

    public static Texture GetLevelIcon(string levelTitle)
    {
        if (LevelIconLookup.TryGetValue(levelTitle, out var texture))
        {
            return texture;
        }

        return null;
    }

    public static Texture GetAvatarIcon(string avatarTitle)
    {
        if (AvatarIconLookup.TryGetValue(avatarTitle, out var texture)) 
        {
            return texture; 
        }

        return null;
    }

    public static void GetResources(Transform resourcesTransform)
    {
        // Menu icon
        var menuIcon = resourcesTransform.Find("Menu Icon");

        if (menuIcon != null)
        {
            MenuIconSprite = menuIcon.GetComponent<SpriteRenderer>().sprite;
        }

        // Level icons
        var levelIcons = resourcesTransform.Find("Level Icons");

        LevelIconLookup = new();

        foreach (var icon in levelIcons)
        {
            var iconTransform = icon.TryCast<Transform>();

            if (iconTransform == null)
            {
                continue;
            }

            var image = iconTransform.GetComponent<RawImage>();

            if (image == null)
            {
                continue;
            }

            LevelIconLookup.Add(iconTransform.name, image.texture);
        }

        // Avatar icons
        var avatarIcons = resourcesTransform.Find("Avatar Icons");

        AvatarIconLookup = new();

        foreach (var icon in avatarIcons)
        {
            var iconTransform = icon.TryCast<Transform>();

            if (iconTransform == null)
            {
                continue;
            }

            var image = iconTransform.GetComponent<RawImage>();

            if (image == null)
            {
                continue;
            }

            AvatarIconLookup.Add(iconTransform.name, image.texture);
        }
    }
}
