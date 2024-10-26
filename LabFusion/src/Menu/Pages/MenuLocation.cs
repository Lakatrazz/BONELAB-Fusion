using LabFusion.Downloading.ModIO;
using LabFusion.Entities;
using LabFusion.Marrow;
using LabFusion.Marrow.Proxies;
using LabFusion.Network;
using LabFusion.Player;
using LabFusion.Preferences.Server;
using LabFusion.Representation;
using LabFusion.Scene;
using LabFusion.Senders;
using LabFusion.Utilities;

using UnityEngine;

namespace LabFusion.Menu;

public static class MenuLocation
{
    public static MenuPage LocationPage { get; private set; }

    public static LobbyElement LobbyElement { get; private set; }

    public static BoolElement NameTagsElement { get; private set; }
    public static BoolElement VoiceChatElement { get; private set; }
    public static EnumElement SlowMoElement { get; private set; }

    public static void OnInitializeMelon()
    {
        MultiplayerHooking.OnStartServer += OnConnect;
        MultiplayerHooking.OnJoinServer += OnConnect;
        MultiplayerHooking.OnDisconnect += OnDisconnect;

        ServerSettingsManager.OnServerSettingsChanged += OnServerSettingsChanged;
    }

    private static void OnConnect()
    {
        if (LobbyElement == null)
        {
            return;
        }

        PopulateLobbyAsServer(LobbyElement);
    }

    private static void OnDisconnect()
    {
        if (LobbyElement == null)
        {
            return;
        }

        PopulateLobbyNoServer(LobbyElement);
    }

    private static void PopulateLobbyNoServer(LobbyElement element)
    {
        element.ServerActionElement
            .Cleared()
            .WithTitle("Create Server")
            .Do(NetworkHelper.StartServer);

        UpdateLevelIcon(element);

        OnServerSettingsChanged();
    }

    private static void PopulateLobbyAsServer(LobbyElement element)
    {
        element.ServerActionElement
            .Cleared()
            .WithTitle("Disconnect")
            .Do(() => { NetworkHelper.Disconnect(); });

        UpdateLevelIcon(element);

        OnServerSettingsChanged();
    }

    private static void UpdateLevelIcon(LobbyElement element)
    {
        var levelName = FusionSceneManager.Title;

        var levelIcon = MenuResources.GetLevelIcon(levelName);

        if (levelIcon == null)
        {
            levelIcon = MenuResources.GetLevelIcon(MenuResources.ModsIconTitle);
        }

        element.LevelIcon.texture = levelIcon;

        if (!FusionSceneManager.Level.Pallet.IsInMarrowGame())
        {
            ApplyModTexture(element);
        }
    }

    private static void ApplyModTexture(LobbyElement element)
    {
        // Get the mod info
        var pallet = FusionSceneManager.Level.Pallet;

        var manifest = CrateFilterer.GetManifest(pallet);

        if (manifest == null)
        {
            return;
        }

        var modListing = manifest.ModListing;

        var modTarget = ModIOManager.GetTargetFromListing(modListing);

        if (modTarget == null)
        {
            return;
        }

        // Get the texture
        ModIOThumbnailDownloader.GetThumbnail((int)modTarget.ModId, (texture) =>
        {
            element.LevelIcon.texture = texture;
        });
    }

    private static void UpdatePlayerIcon(PlayerElement element, string avatarTitle)
    {
        var avatarIcon = MenuResources.GetAvatarIcon(avatarTitle);

        if (avatarIcon == null)
        {
            avatarIcon = MenuResources.GetAvatarIcon(MenuResources.ModsIconTitle);
        }

        element.PlayerIcon.texture = avatarIcon;
    }

    private static void UpdatePlayerIcon(PlayerResultElement element, string avatarTitle)
    {
        var avatarIcon = MenuResources.GetAvatarIcon(avatarTitle);

        if (avatarIcon == null)
        {
            avatarIcon = MenuResources.GetAvatarIcon(MenuResources.ModsIconTitle);
        }

        element.PlayerIcon.texture = avatarIcon;
    }

    private static void OnServerSettingsChanged()
    {
        ApplyServerSettingsToLobby(LobbyElement, ServerSettingsManager.ActiveSettings);
    }

    private static void ApplyServerSettingsToLobby(LobbyElement element, ServerSettings settings)
    {
        bool ownsSettings = NetworkInfo.IsServer || !NetworkInfo.HasServer;

        string emptyFormat = ownsSettings ? "Click to add {0}" : "No {0}";

        element.LevelNameElement
            .WithTitle(FusionSceneManager.Title);

        element.ServerVersionElement
            .WithTitle($"v{FusionMod.Version}");

        var playerCount = PlayerIdManager.PlayerCount;

        element.PlayersElement
            .Cleared()
            .AsPref(settings.MaxPlayers)
            .WithIncrement(1)
            .WithLimits(2, 255)
            .WithTitle("Players");

        element.PlayersElement.TextFormat = $"{playerCount}/{{1}} {{0}}";

        element.PrivacyElement
            .Cleared()
            .AsPref(settings.Privacy)
            .WithTitle("Privacy");

        element.PrivacyElement.TextFormat = "{1}";

        element.ServerNameElement
            .Cleared()
            .AsPref(settings.ServerName)
            .WithTitle("Server Name");

        element.ServerNameElement.EmptyFormat = emptyFormat;
        element.ServerNameElement.TextFormat = "{1}";

        element.HostNameElement
            .WithTitle($"{PlayerIdManager.LocalUsername}");

        element.DescriptionElement
            .Cleared()
            .AsPref(settings.ServerDescription)
            .WithTitle("Description");

        element.DescriptionElement.EmptyFormat = emptyFormat;
        element.DescriptionElement.TextFormat = "{1}";

        element.MoreElement
            .Cleared()
            .WithTitle("More...")
            .Do(() => { element.LobbyPage.SelectSubPage(1); });

        // Fill out lists
        // Settings list
        NameTagsElement
            .Cleared()
            .WithInteractability(ownsSettings)
            .AsPref(settings.NametagsEnabled)
            .WithTitle("NameTags");

        VoiceChatElement
            .Cleared()
            .WithInteractability(ownsSettings)
            .AsPref(settings.VoiceChatEnabled)
            .WithTitle("VoiceChat");

        SlowMoElement
            .Cleared()
            .WithInteractability(ownsSettings)
            .AsPref(settings.TimeScaleMode)
            .WithTitle("SlowMo");

        RefreshPlayerList();

        // Change interactability for all elements
        element.Interactable = ownsSettings;
    }

    private static void RefreshPlayerList()
    {
        var element = LobbyElement;

        element.PlayerBrowserElement.Clear();

        var playerListPage = element.PlayerBrowserElement.AddPage();

        foreach (var player in PlayerIdManager.PlayerIds)
        {
            MetadataHelper.TryGetDisplayName(player, out var name);

            var playerResult = playerListPage.AddElement<PlayerResultElement>(name);

            playerResult.GetReferences();

            playerResult.PlayerNameText.text = name;

            playerResult.RoleText.text = "User";

            playerResult.OnPressed = () =>
            {
                OnShowPlayer(player);
            };

            // Apply icon
            var avatarTitle = "PolyBlank";

            if (NetworkPlayerManager.TryGetPlayer(player, out var networkPlayer) && networkPlayer.HasRig)
            {
                avatarTitle = networkPlayer.RigRefs.RigManager.AvatarCrate.Crate.Title;
            }

            UpdatePlayerIcon(playerResult, avatarTitle);
        }
    }

    private static void OnShowPlayer(PlayerId player)
    {
        if (!player.IsValid)
        {
            return;
        }

        LobbyElement.LobbyPage.SelectSubPage(2);

        ApplyPlayerToElement(LobbyElement.ProfileElement, player);
    }

    private static void ApplyPlayerToElement(PlayerElement element, PlayerId player)
    {
        // Apply name and description
        var username = player.Metadata.GetMetadata(MetadataHelper.UsernameKey);
        element.UsernameElement.Title = username;

        element.NicknameElement.Title = "Nickname";
        element.NicknameElement.Value = player.Metadata.GetMetadata(MetadataHelper.NicknameKey);
        element.NicknameElement.Interactable = false;
        element.NicknameElement.EmptyFormat = "No {0}";

        element.DescriptionElement.Title = "Description";
        element.DescriptionElement.Interactable = false;
        element.DescriptionElement.EmptyFormat = "No {0}";

        // Apply icon
        var avatarTitle = "PolyBlank";

        if (NetworkPlayerManager.TryGetPlayer(player, out var networkPlayer) && networkPlayer.HasRig)
        {
            avatarTitle = networkPlayer.RigRefs.RigManager.AvatarCrate.Crate.Title;
        }

        UpdatePlayerIcon(element, avatarTitle);

        // Get permissions
        FusionPermissions.FetchPermissionLevel(PlayerIdManager.LocalLongId, out var selfLevel, out _);

        FusionPermissions.FetchPermissionLevel(player.LongId, out var level, out Color color);

        var serverSettings = ServerSettingsManager.ActiveSettings;

        // Permissions element
        var permissionsElement = element.PermissionsElement
            .WithTitle("Permissions")
            .WithColor(Color.yellow);

        permissionsElement.EnumType = typeof(PermissionLevel);
        permissionsElement.Value = level;

        permissionsElement.OnValueChanged += (v) =>
        {
            FusionPermissions.TrySetPermission(player.LongId, username, (PermissionLevel)v);
        };

        permissionsElement.Interactable = !player.IsMe && NetworkInfo.IsServer;

        // Actions
        element.ActionsElement.Clear();
        var actionsPage = element.ActionsElement.AddPage();

        if (!player.IsMe && (NetworkInfo.IsServer || FusionPermissions.HasHigherPermissions(selfLevel, level)))
        {
            var moderationGroup = actionsPage.AddElement<GroupElement>("Moderation");

            // Kick button
            if (FusionPermissions.HasSufficientPermissions(selfLevel, serverSettings.KickingAllowed.Value))
            {
                moderationGroup.AddElement<FunctionElement>("Kick")
                    .WithColor(Color.red)
                    .Do(() =>
                    {
                        PermissionSender.SendPermissionRequest(PermissionCommandType.KICK, player);
                    });
            }

            // Ban button
            if (FusionPermissions.HasSufficientPermissions(selfLevel, serverSettings.BanningAllowed.Value))
            {
                moderationGroup.AddElement<FunctionElement>("Ban")
                    .WithColor(Color.red)
                    .Do(() =>
                    {
                        PermissionSender.SendPermissionRequest(PermissionCommandType.BAN, player);
                    });
            }

            // Teleport buttons
            if (FusionPermissions.HasSufficientPermissions(selfLevel, serverSettings.Teleportation.Value))
            {
                moderationGroup.AddElement<FunctionElement>("Teleport To Them")
                    .WithColor(Color.red)
                    .Do(() =>
                    {
                        PermissionSender.SendPermissionRequest(PermissionCommandType.TELEPORT_TO_THEM, player);
                    });

                moderationGroup.AddElement<FunctionElement>("Teleport To Us")
                    .WithColor(Color.red)
                    .Do(() =>
                    {
                        PermissionSender.SendPermissionRequest(PermissionCommandType.TELEPORT_TO_US, player);
                    });
            }
        }
    }

    public static void PopulateLocation(GameObject locationPage)
    {
        LocationPage = locationPage.GetComponent<MenuPage>();

        LobbyElement = locationPage.transform.Find("panel_Lobby").GetComponent<LobbyElement>();

        LobbyElement.GetElements();

        LobbyElement.LobbyPage.OnShown += () =>
        {
            RefreshPlayerList();
        };

        var settingsPage = LobbyElement.SettingsElement.AddPage();
        PopulateSettings(settingsPage);

        // Update server status
        if (NetworkInfo.HasServer)
        {
            OnConnect();
        }
        else
        {
            OnDisconnect();
        }
    }

    private static void PopulateSettings(PageElement element)
    {
        var generalGroup = element.AddElement<GroupElement>("General");

        NameTagsElement = generalGroup.AddElement<BoolElement>("NameTags");

        VoiceChatElement = generalGroup.AddElement<BoolElement>("VoiceChat");

        SlowMoElement = generalGroup.AddElement<EnumElement>("SlowMo");
    }
}