using UnityEngine;
using UnityEngine.UI;

namespace LabFusion.Menu;

public static class MenuResources
{
    public static Sprite MenuIconSprite { get; private set; } = null;

    public static Dictionary<string, Texture> LevelIconLookup { get; private set; } = null;
    public static Dictionary<string, Texture> AvatarIconLookup { get; private set; } = null;
    public static Dictionary<string, Texture> GamemodeIconLookup { get; private set; } = null;

    public static Dictionary<string, Texture> PointIconLookup { get; private set; } = null;

    public const string ModsIconTitle = "Mods";

    public const string EmptyIconTitle = "Empty";

    public const string SandboxIconTitle = "Sandbox";

    public static Texture GetLevelIcon(string levelTitle)
    {
        if (string.IsNullOrWhiteSpace(levelTitle))
        {
            return null;
        }

        if (LevelIconLookup.TryGetValue(levelTitle, out var texture))
        {
            return texture;
        }

        return null;
    }

    public static Texture GetAvatarIcon(string avatarTitle)
    {
        if (string.IsNullOrWhiteSpace(avatarTitle))
        {
            return null;
        }

        if (AvatarIconLookup.TryGetValue(avatarTitle, out var texture)) 
        {
            return texture; 
        }

        return null;
    }

    public static Texture GetGamemodeIcon(string gamemodeTitle)
    {
        if (string.IsNullOrWhiteSpace(gamemodeTitle))
        {
            return null;
        }

        if (GamemodeIconLookup.TryGetValue(gamemodeTitle, out var texture))
        {
            return texture;
        }

        return null;
    }

    public static Texture GetPointIcon(string pointTitle)
    {
        if (string.IsNullOrWhiteSpace(pointTitle))
        {
            return null;
        }

        if (PointIconLookup.TryGetValue(pointTitle, out var texture))
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

        LevelIconLookup = LoadIcons(resourcesTransform, "Level Icons");
        AvatarIconLookup = LoadIcons(resourcesTransform, "Avatar Icons");
        GamemodeIconLookup = LoadIcons(resourcesTransform, "Gamemode Icons");
        PointIconLookup = LoadIcons(resourcesTransform, "Point Icons");
    }

    private static Dictionary<string, Texture> LoadIcons(Transform resources, string name)
    {
        var icons = resources.Find(name);

        var lookup = new Dictionary<string, Texture>();

        foreach (var icon in icons)
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

            lookup.Add(iconTransform.name, image.texture);
        }

        return lookup;
    }
}
