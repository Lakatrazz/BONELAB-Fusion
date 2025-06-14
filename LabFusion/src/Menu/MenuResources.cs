using UnityEngine;
using UnityEngine.UI;

namespace LabFusion.Menu;

public static class MenuResources
{
    public static bool HasResources => ResourcesTransform != null;

    public static Transform ResourcesTransform { get; private set; } = null;

    public static Sprite MenuIconSprite { get; private set; } = null;

    public static Dictionary<string, Texture> LevelIconLookup { get; private set; } = null;
    public static Dictionary<string, Texture> AvatarIconLookup { get; private set; } = null;
    public static Dictionary<string, Texture> GamemodeIconLookup { get; private set; } = null;

    public static Dictionary<string, Texture> PointIconLookup { get; private set; } = null;
    public static Dictionary<string, Texture> NotificationIconLookup { get; private set; } = null;
    public static Dictionary<string, Texture> LogoIconLookup { get; private set; } = null;
    public static Dictionary<string, Texture> AchievementIconLookup { get; private set; } = null;

    public const string ModsIconTitle = "Mods";

    public const string EmptyIconTitle = "Empty";

    public const string SandboxIconTitle = "Sandbox";

    private static Action _onResourcesReadyCallback = null;

    public static Texture GetLevelIcon(string levelTitle) => GetIcon(LevelIconLookup, levelTitle);

    public static Texture GetAvatarIcon(string avatarTitle) => GetIcon(AvatarIconLookup, avatarTitle);

    public static Texture GetGamemodeIcon(string gamemodeTitle) => GetIcon(GamemodeIconLookup, gamemodeTitle);

    public static Texture GetPointIcon(string pointTitle) => GetIcon(PointIconLookup, pointTitle);

    public static Texture GetNotificationIcon(string notificationType) => GetIcon(NotificationIconLookup, notificationType);

    public static Texture GetLogoIcon(string logoTitle) => GetIcon(LogoIconLookup, logoTitle);

    public static Texture GetAchievementIcon(string achievementTitle) => GetIcon(AchievementIconLookup, achievementTitle);

    private static Texture GetIcon(Dictionary<string, Texture> lookup, string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return null;
        }

        if (lookup.TryGetValue(key, out var texture))
        {
            return texture;
        }

        return null;
    }

    public static void GetResources(Transform resourcesTransform)
    {
        ResourcesTransform = resourcesTransform;

        // Menu icon
        var menuIcon = ResourcesTransform.Find("Menu Icon");

        if (menuIcon != null)
        {
            MenuIconSprite = menuIcon.GetComponent<SpriteRenderer>().sprite;
        }

        LevelIconLookup = LoadIcons(ResourcesTransform, "Level Icons");
        AvatarIconLookup = LoadIcons(ResourcesTransform, "Avatar Icons");
        GamemodeIconLookup = LoadIcons(ResourcesTransform, "Gamemode Icons");

        PointIconLookup = LoadIcons(ResourcesTransform, "Point Icons");
        NotificationIconLookup = LoadIcons(ResourcesTransform, "Notification Icons");
        LogoIconLookup = LoadIcons(ResourcesTransform, "Logo Icons");
        AchievementIconLookup = LoadIcons(ResourcesTransform, "Achievement Icons");

        _onResourcesReadyCallback?.Invoke();
        _onResourcesReadyCallback = null;
    }

    public static void HookResourcesReady(Action callback)
    {
        if (HasResources)
        {
            callback?.Invoke();
        }
        else
        {
            _onResourcesReadyCallback += callback;
        }
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
