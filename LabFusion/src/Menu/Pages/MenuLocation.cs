using LabFusion.Data;
using LabFusion.Entities;
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

        LobbyInfoManager.OnLobbyInfoChanged += OnServerSettingsChanged;
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
        ElementIconHelper.SetLevelIcon(element, FusionSceneManager.Title, ElementIconHelper.GetModId(FusionSceneManager.Level.Pallet));
    }

    private static void OnServerSettingsChanged()
    {
        // Make sure the lobby element has been created
        if (LobbyElement == null)
        {
            return;
        }

        if (NetworkInfo.IsClient)
        {
            ApplyLobbyInfoToLobby(LobbyElement, LobbyInfoManager.LobbyInfo);
        }
        else
        {
            ApplyServerSettingsToLobby(LobbyElement);
        }
    }

    private static void ApplyLobbyInfoToLobby(LobbyElement element, LobbyInfo info)
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
            .WithIncrement(1)
            .WithLimits(2, 255)
            .WithTitle("Players");

        element.PlayersElement.Value = info.MaxPlayers;

        element.PlayersElement.TextFormat = $"{playerCount}/{{1}} {{0}}";

        element.PrivacyElement
            .Cleared()
            .WithTitle("Privacy");

        element.PrivacyElement.Value = info.Privacy;

        element.PrivacyElement.TextFormat = "{1}";

        element.ServerNameElement
            .Cleared()
            .WithTitle("Server Name");

        element.ServerNameElement.Value = info.LobbyName;

        element.ServerNameElement.EmptyFormat = emptyFormat;
        element.ServerNameElement.TextFormat = "{1}";

        element.HostNameElement
            .WithTitle($"{PlayerIdManager.LocalUsername}");

        element.DescriptionElement
            .Cleared()
            .WithTitle("Description");

        element.DescriptionElement.Value = info.LobbyDescription;

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
            .WithTitle("NameTags");

        NameTagsElement.Value = info.NameTags;

        VoiceChatElement
            .Cleared()
            .WithInteractability(ownsSettings)
            .WithTitle("VoiceChat");

        VoiceChatElement.Value = info.VoiceChat;

        SlowMoElement
            .Cleared()
            .WithInteractability(ownsSettings)
            .WithTitle("SlowMo");

        SlowMoElement.Value = info.SlowMoMode;

        RefreshPlayerList();

        // Show server code
        element.CodeElement
            .Cleared()
            .WithTitle("Code")
            .WithInteractability(false);

        element.CodeElement.Value = NetworkHelper.GetServerCode();
        element.CodeElement.EmptyFormat = "No {0}";

        element.CodeRefreshElement
            .Cleared()
            .WithInteractability(ownsSettings)
            .Do(NetworkHelper.RefreshServerCode);

        // Disable unnecessary elements
        element.BansGrid.SetActive(ownsSettings);
        element.AdminGrid.SetActive(ownsSettings);

        // This also shouldn't show while not in a server
        element.CodeGrid.SetActive(NetworkInfo.IsServer);

        // Change interactability for all elements
        element.Interactable = ownsSettings;
    }

    private static void ApplyServerSettingsToLobby(LobbyElement element)
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
            .AsPref(SavedServerSettings.MaxPlayers)
            .WithIncrement(1)
            .WithLimits(2, 255)
            .WithTitle("Players");

        element.PlayersElement.TextFormat = $"{playerCount}/{{1}} {{0}}";

        element.PrivacyElement
            .Cleared()
            .AsPref(SavedServerSettings.Privacy)
            .WithTitle("Privacy");

        element.PrivacyElement.TextFormat = "{1}";

        element.ServerNameElement
            .Cleared()
            .AsPref(SavedServerSettings.ServerName)
            .WithTitle("Server Name");

        element.ServerNameElement.EmptyFormat = emptyFormat;
        element.ServerNameElement.TextFormat = "{1}";

        element.HostNameElement
            .WithTitle($"{PlayerIdManager.LocalUsername}");

        element.DescriptionElement
            .Cleared()
            .AsPref(SavedServerSettings.ServerDescription)
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
            .AsPref(SavedServerSettings.NameTags)
            .WithTitle("NameTags");

        VoiceChatElement
            .Cleared()
            .WithInteractability(ownsSettings)
            .AsPref(SavedServerSettings.VoiceChat)
            .WithTitle("VoiceChat");

        SlowMoElement
            .Cleared()
            .WithInteractability(ownsSettings)
            .AsPref(SavedServerSettings.SlowMoMode)
            .WithTitle("SlowMo");

        RefreshPlayerList();

        // Show server code
        element.CodeElement
            .Cleared()
            .WithTitle("Code")
            .WithInteractability(false);

        element.CodeElement.Value = NetworkHelper.GetServerCode();
        element.CodeElement.EmptyFormat = "No {0}";

        element.CodeRefreshElement
            .Cleared()
            .WithInteractability(ownsSettings)
            .Do(NetworkHelper.RefreshServerCode);

        // Disable unnecessary elements
        element.BansGrid.SetActive(ownsSettings);
        element.AdminGrid.SetActive(ownsSettings);

        // This also shouldn't show while not in a server
        element.CodeGrid.SetActive(NetworkInfo.IsServer);

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

            player.TryGetPermissionLevel(out var permissionLevel);
            playerResult.RoleText.text = permissionLevel.ToString();

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

            ElementIconHelper.SetProfileResultIcon(playerResult, avatarTitle);
        }
    }

    private static void RefreshBanList()
    {
        var element = LobbyElement;

        element.BansElement.Clear();

        var banListPage = element.BansElement.AddPage();

        BanManager.ReadFile();

        foreach (var ban in BanManager.BanList.Bans)
        {
            var player = ban.Player;

            var banResult = banListPage.AddElement<PlayerResultElement>(player.Username);

            banResult.GetReferences();

            banResult.PlayerNameText.text = player.Username;

            banResult.RoleText.text = "Banned";

            banResult.OnPressed = () =>
            {
                OnShowBannedPlayer(player);
            };

            // Apply icon
            ElementIconHelper.SetProfileResultIcon(banResult, player.AvatarTitle, player.AvatarModId);
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

    private static void OnShowBannedPlayer(PlayerInfo playerInfo)
    {
        LobbyElement.LobbyPage.SelectSubPage(2);

        ApplyBannedPlayerToElement(LobbyElement.ProfileElement, playerInfo);
    }

    private static void ApplyBannedPlayerToElement(PlayerElement element, PlayerInfo playerInfo)
    {
        // Apply name and description
        var username = playerInfo.Username;
        element.UsernameElement.Title = username;

        element.NicknameElement.Title = "Nickname";
        element.NicknameElement.Value = playerInfo.Nickname;
        element.NicknameElement.Interactable = false;
        element.NicknameElement.EmptyFormat = "No {0}";

        element.DescriptionElement.Title = "Description";
        element.DescriptionElement.Interactable = false;
        element.DescriptionElement.EmptyFormat = "No {0}";

        // Apply icon
        var avatarTitle = playerInfo.AvatarTitle;

        ElementIconHelper.SetProfileIcon(element, avatarTitle, playerInfo.AvatarModId);

        var activeLobbyInfo = LobbyInfoManager.LobbyInfo;

        // Actions
        element.ActionsElement.Clear();
        var actionsPage = element.ActionsElement.AddPage();

        var moderationGroup = actionsPage.AddElement<GroupElement>("Moderation");

        moderationGroup.AddElement<FunctionElement>("Unban")
            .WithColor(Color.yellow)
            .Do(() =>
            {
                NetworkHelper.PardonUser(playerInfo.LongId);
            });

        // Disable unnecessary elements
        element.PermissionsElement.gameObject.SetActive(false);
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

        ElementIconHelper.SetProfileIcon(element, avatarTitle);

        // Get permissions
        FusionPermissions.FetchPermissionLevel(PlayerIdManager.LocalLongId, out var selfLevel, out _);

        FusionPermissions.FetchPermissionLevel(player.LongId, out var level, out Color color);

        var activeLobbyInfo = LobbyInfoManager.LobbyInfo;

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
            if (FusionPermissions.HasSufficientPermissions(selfLevel, activeLobbyInfo.Kicking))
            {
                moderationGroup.AddElement<FunctionElement>("Kick")
                    .WithColor(Color.red)
                    .Do(() =>
                    {
                        PermissionSender.SendPermissionRequest(PermissionCommandType.KICK, player);
                    });
            }

            // Ban button
            if (FusionPermissions.HasSufficientPermissions(selfLevel, activeLobbyInfo.Banning))
            {
                moderationGroup.AddElement<FunctionElement>("Ban")
                    .WithColor(Color.red)
                    .Do(() =>
                    {
                        PermissionSender.SendPermissionRequest(PermissionCommandType.BAN, player);
                    });
            }

            // Teleport buttons
            if (FusionPermissions.HasSufficientPermissions(selfLevel, activeLobbyInfo.Teleportation))
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
            RefreshBanList();
        };

        var settingsPage = LobbyElement.SettingsElement.AddPage();
        PopulateSettings(settingsPage);

        var adminPage = LobbyElement.AdminElement.AddPage();
        PopulateAdmin(adminPage);

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

    private static void PopulateAdmin(PageElement element)
    {
        var cleanupGroup = element.AddElement<GroupElement>("Cleanup");

        var despawnAllElement = cleanupGroup.AddElement<FunctionElement>("Despawn All")
            .Do(() =>
            {
                PooleeUtilities.DespawnAll();
            });
    }
}