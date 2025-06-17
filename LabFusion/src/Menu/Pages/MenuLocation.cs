using LabFusion.Data;
using LabFusion.Entities;
using LabFusion.Extensions;
using LabFusion.Marrow;
using LabFusion.Marrow.Proxies;
using LabFusion.Network;
using LabFusion.Player;
using LabFusion.Preferences.Server;
using LabFusion.Representation;
using LabFusion.Safety;
using LabFusion.Scene;
using LabFusion.SDK.Gamemodes;
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
    public static BoolElement MortalityElement { get; private set; }
    public static BoolElement FriendlyFireElement { get; private set; }
    public static BoolElement KnockoutElement { get; private set; }
    public static IntElement KnockoutLengthElement { get; private set; }
    public static BoolElement PlayerConstrainingElement { get; private set; }
    public static FloatElement MaxAvatarHeightElement { get; private set; }

    public static EnumElement DevToolsElement { get; private set; }
    public static EnumElement ConstrainerElement { get; private set; }
    public static EnumElement CustomAvatarsElement { get; private set; }
    public static EnumElement KickingElement { get; private set; }
    public static EnumElement BanningElement { get; private set; }
    public static EnumElement TeleportationElement { get; private set; }

    public static void OnInitializeMelon()
    {
        MultiplayerHooking.OnStartedServer += OnConnect;
        MultiplayerHooking.OnJoinedServer += OnConnect;
        MultiplayerHooking.OnDisconnected += OnDisconnect;

        LobbyInfoManager.OnLobbyInfoChanged += OnServerSettingsChanged;
        LocalPlayer.OnUsernameChanged += OnUsernameChanged;
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

        UpdateLobbyIcons(element);

        OnServerSettingsChanged();
    }

    private static void PopulateLobbyAsServer(LobbyElement element)
    {
        element.ServerActionElement
            .Cleared()
            .WithTitle("Disconnect")
            .Do(() => { NetworkHelper.Disconnect(); });

        UpdateLobbyIcons(element);

        OnServerSettingsChanged();
    }

    private static void UpdateLobbyIcons(LobbyElement element)
    {
        ElementIconHelper.SetLevelIcon(element, FusionSceneManager.Title, CrateFilterer.GetModID(FusionSceneManager.Level.Pallet));
        ElementIconHelper.SetGamemodeIcon(element, GamemodeManager.ActiveGamemode?.Title);
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

    private static void OnUsernameChanged(string value)
    {
        if (LobbyElement == null)
        {
            return;
        }

        if (NetworkInfo.IsClient)
        {
            return;
        }

        LobbyElement.HostNameElement
            .WithTitle(LocalPlayer.Username);
    }

    private static void ApplyLobbyInfoToLobby(LobbyElement element, LobbyInfo info)
    {
        bool ownsSettings = NetworkInfo.IsHost || !NetworkInfo.HasServer;

        string emptyFormat = ownsSettings ? "Click to add {0}" : "No {0}";

        element.LevelNameElement
            .WithTitle(FusionSceneManager.Title);

        element.ServerVersionElement
            .WithTitle($"v{FusionMod.Version}");

        var playerCount = PlayerIDManager.PlayerCount;

        element.PlayersElement
            .Cleared()
            .WithIncrement(1)
            .WithLimits(2, 255)
            .WithTitle("Players")
            .WithValue(info.MaxPlayers);

        element.PlayersElement.TextFormat = $"{playerCount}/{{1}} {{0}}";

        element.PrivacyElement
            .Cleared()
            .WithTitle("Privacy")
            .WithValue(info.Privacy);

        element.PrivacyElement.TextFormat = "{1}";

        element.ServerNameElement
            .Cleared()
            .WithTitle("Server Name")
            .WithValue(info.LobbyName.RemoveRichTextExceptColor());

        element.ServerNameElement.EmptyFormat = emptyFormat;
        element.ServerNameElement.TextFormat = "{1}";

        element.HostNameElement
            .WithTitle(info.LobbyHostName);

        element.DescriptionElement
            .Cleared()
            .WithTitle("Description")
            .WithValue(info.LobbyDescription.RemoveRichTextExceptColor());

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
            .WithTitle("NameTags")
            .WithValue(info.NameTags);

        VoiceChatElement
            .Cleared()
            .WithInteractability(ownsSettings)
            .WithTitle("VoiceChat")
            .WithValue(info.VoiceChat);

        SlowMoElement
            .Cleared()
            .WithInteractability(ownsSettings)
            .WithTitle("SlowMo")
            .WithValue(info.SlowMoMode);

        MortalityElement
            .Cleared()
            .WithInteractability(ownsSettings)
            .WithTitle("Mortality")
            .WithValue(info.Mortality);

        FriendlyFireElement
            .Cleared()
            .WithInteractability(ownsSettings)
            .WithTitle("Friendly Fire")
            .WithValue(info.FriendlyFire);

        KnockoutElement
            .Cleared()
            .WithInteractability(ownsSettings)
            .WithTitle("Knockout")
            .WithValue(info.Knockout);

        KnockoutLengthElement
            .Cleared()
            .WithInteractability(ownsSettings)
            .WithTitle("Knockout Length")
            .WithValue(info.KnockoutLength);

        PlayerConstrainingElement
            .Cleared()
            .WithInteractability(ownsSettings)
            .WithTitle("Player Constraining")
            .WithValue(info.PlayerConstraining);

        MaxAvatarHeightElement
            .Cleared()
            .WithInteractability(ownsSettings)
            .WithTitle("Max Avatar Height")
            .WithValue(info.MaxAvatarHeight);

        // Permissions
        DevToolsElement
            .Cleared()
            .WithInteractability(ownsSettings)
            .WithTitle("Dev Tools")
            .WithValue(info.DevTools);
        ConstrainerElement
            .Cleared()
            .WithInteractability(ownsSettings)
            .WithTitle("Constrainer")
            .WithValue(info.Constrainer);
        CustomAvatarsElement
            .Cleared()
            .WithInteractability(ownsSettings)
            .WithTitle("Custom Avatars")
            .WithValue(info.CustomAvatars);
        KickingElement
            .Cleared()
            .WithInteractability(ownsSettings)
            .WithTitle("Kicking")
            .WithValue(info.Kicking);
        BanningElement
            .Cleared()
            .WithInteractability(ownsSettings)
            .WithTitle("Banning")
            .WithValue(info.Banning);
        TeleportationElement
            .Cleared()
            .WithInteractability(ownsSettings)
            .WithTitle("Teleportation")
            .WithValue(info.Teleportation);

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
        element.CodeGrid.SetActive(NetworkInfo.IsHost);

        // Change interactability for all elements
        element.Interactable = ownsSettings;
    }

    private static void ApplyServerSettingsToLobby(LobbyElement element)
    {
        bool ownsSettings = NetworkInfo.IsHost || !NetworkInfo.HasServer;

        string emptyFormat = ownsSettings ? "Click to add {0}" : "No {0}";

        element.LevelNameElement
            .WithTitle(FusionSceneManager.Title);

        element.ServerVersionElement
            .WithTitle($"v{FusionMod.Version}");

        var playerCount = PlayerIDManager.PlayerCount;

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
            .WithTitle($"{LocalPlayer.Username}");

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

        MortalityElement
            .Cleared()
            .WithInteractability(ownsSettings)
            .AsPref(SavedServerSettings.Mortality)
            .WithTitle("Mortality");

        FriendlyFireElement
            .Cleared()
            .WithInteractability(ownsSettings)
            .AsPref(SavedServerSettings.FriendlyFire)
            .WithTitle("Friendly Fire");

        KnockoutElement
            .Cleared()
            .WithInteractability(ownsSettings)
            .AsPref(SavedServerSettings.Knockout)
            .WithTitle("Knockout");

        KnockoutLengthElement
            .Cleared()
            .WithInteractability(ownsSettings)
            .AsPref(SavedServerSettings.KnockoutLength)
            .WithLimits(5, 300)
            .WithIncrement(5)
            .WithTitle("Knockout Length");

        PlayerConstrainingElement
            .Cleared()
            .WithInteractability(ownsSettings)
            .AsPref(SavedServerSettings.PlayerConstraining)
            .WithTitle("Player Constraining");

        MaxAvatarHeightElement
            .Cleared()
            .WithInteractability(ownsSettings)
            .AsPref(SavedServerSettings.MaxAvatarHeight)
            .WithLimits(2f, 30f)
            .WithIncrement(1f)
            .WithTitle("Max Avatar Height");

        // Permissions
        DevToolsElement
            .Cleared()
            .WithInteractability(ownsSettings)
            .AsPref(SavedServerSettings.DevTools)
            .WithTitle("Dev Tools");

        ConstrainerElement
            .Cleared()
            .WithInteractability(ownsSettings)
            .AsPref(SavedServerSettings.Constrainer)
            .WithTitle("Constrainer");

        CustomAvatarsElement
            .Cleared()
            .WithInteractability(ownsSettings)
            .AsPref(SavedServerSettings.CustomAvatars)
            .WithTitle("Custom Avatars");

        KickingElement
            .Cleared()
            .WithInteractability(ownsSettings)
            .AsPref(SavedServerSettings.Kicking)
            .WithTitle("Kicking");

        BanningElement
            .Cleared()
            .WithInteractability(ownsSettings)
            .AsPref(SavedServerSettings.Banning)
            .WithTitle("Banning");

        TeleportationElement
            .Cleared()
            .WithInteractability(ownsSettings)
            .AsPref(SavedServerSettings.Teleportation)
            .WithTitle("Teleportation");

        // Update player list
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
        element.CodeGrid.SetActive(NetworkInfo.IsHost);

        // Change interactability for all elements
        element.Interactable = ownsSettings;
    }

    private static void RefreshPlayerList()
    {
        var element = LobbyElement;

        element.PlayerBrowserElement.Clear();

        var playerListPage = element.PlayerBrowserElement.AddPage();

        foreach (var player in PlayerIDManager.PlayerIDs)
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
            var avatarTitle = player.Metadata.AvatarTitle.GetValue();
            var modId = player.Metadata.AvatarModID.GetValue();

            if (NetworkPlayerManager.TryGetPlayer(player, out var networkPlayer) && networkPlayer.HasRig)
            {
                avatarTitle = networkPlayer.RigRefs.RigManager.AvatarCrate.Crate.Title;
            }

            ElementIconHelper.SetProfileResultIcon(playerResult, avatarTitle, modId);
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

    private static void OnShowPlayer(PlayerID player)
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
        element.UsernameElement.Title = username.RemoveRichTextExceptColor();

        element.NicknameElement.Title = "Nickname";
        element.NicknameElement.Value = playerInfo.Nickname.RemoveRichTextExceptColor();
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

    private static void ApplyPlayerToElement(PlayerElement element, PlayerID player)
    {
        // Apply name and description
        var username = TextFilter.Filter(player.Metadata.Username.GetValue());
        element.UsernameElement.Title = username;

        element.NicknameElement.Title = "Nickname";
        element.NicknameElement.Value = TextFilter.Filter(player.Metadata.Nickname.GetValue());
        element.NicknameElement.Interactable = false;
        element.NicknameElement.EmptyFormat = "No {0}";

        element.DescriptionElement.Title = "Description";
        element.DescriptionElement.Value = TextFilter.Filter(player.Metadata.Description.GetValue());
        element.DescriptionElement.Interactable = false;
        element.DescriptionElement.EmptyFormat = "No {0}";

        // Apply icon
        var avatarTitle = player.Metadata.AvatarTitle.GetValue();
        var modId = player.Metadata.AvatarModID.GetValue();

        ElementIconHelper.SetProfileIcon(element, avatarTitle, modId);

        // Get permissions
        FusionPermissions.FetchPermissionLevel(PlayerIDManager.LocalPlatformID, out var selfLevel, out _);

        FusionPermissions.FetchPermissionLevel(player.PlatformID, out var level, out Color color);

        var activeLobbyInfo = LobbyInfoManager.LobbyInfo;

        // Volume element
        if (player.IsMe)
        {
            element.VolumeElement.gameObject.SetActive(false);
        }
        else
        {
            var volumeElement = element.VolumeElement
                .Cleared()
                .WithTitle("Volume")
                .WithIncrement(0.1f)
                .WithLimits(0f, 2f);

            volumeElement.gameObject.SetActive(true);

            volumeElement.Value = ContactsList.GetContact(player).volume;
            volumeElement.OnValueChanged += (v) =>
            {
                var contact = ContactsList.GetContact(player);
                contact.volume = v;
                ContactsList.UpdateContact(contact);
            };
        }

        // Permissions element
        var permissionsElement = element.PermissionsElement
            .Cleared()
            .WithTitle("Permissions")
            .WithColor(Color.yellow);

        permissionsElement.EnumType = typeof(PermissionLevel);
        permissionsElement.Value = level;

        permissionsElement.OnValueChanged += (v) =>
        {
            FusionPermissions.TrySetPermission(player.PlatformID, username, (PermissionLevel)v);
        };

        permissionsElement.Interactable = !player.IsMe && NetworkInfo.IsHost;

        // Platform ID element
        var platformIDElement = element.PlatformIDElement
            .Cleared()
            .WithTitle("Platform ID")
            .WithColor(Color.red)
            .WithInteractability(false);

        platformIDElement.Value = player.PlatformID.ToString();

        // Actions
        element.ActionsElement.Clear();
        var actionsPage = element.ActionsElement.AddPage();

        AddModerationGroup(activeLobbyInfo, actionsPage, player, selfLevel, level);
    }

    private static void AddModerationGroup(LobbyInfo lobbyInfo, PageElement actionsPage, PlayerID player, PermissionLevel selfLevel, PermissionLevel level)
    {
        if (player.IsMe)
        {
            return;
        }

        bool sufficientPerms = FusionPermissions.HasSufficientPermissions(selfLevel, level) || NetworkInfo.IsHost;
        bool higherPerms = FusionPermissions.HasHigherPermissions(selfLevel, level) || NetworkInfo.IsHost;

        if (!sufficientPerms && !higherPerms)
        {
            return;
        }

        GroupElement moderationGroup = null;

        // Kick button
        if (higherPerms && FusionPermissions.HasSufficientPermissions(selfLevel, lobbyInfo.Kicking))
        {
            moderationGroup ??= actionsPage.AddElement<GroupElement>("Moderation");

            moderationGroup.AddElement<FunctionElement>("Kick")
                .WithColor(Color.red)
                .Do(() =>
                {
                    PermissionSender.SendPermissionRequest(PermissionCommandType.KICK, player);
                });
        }

        // Ban button
        if (higherPerms && FusionPermissions.HasSufficientPermissions(selfLevel, lobbyInfo.Banning))
        {
            moderationGroup ??= actionsPage.AddElement<GroupElement>("Moderation");

            moderationGroup.AddElement<FunctionElement>("Ban")
                .WithColor(Color.red)
                .Do(() =>
                {
                    PermissionSender.SendPermissionRequest(PermissionCommandType.BAN, player);
                });
        }

        // Teleport buttons
        if (FusionPermissions.HasSufficientPermissions(selfLevel, lobbyInfo.Teleportation))
        {
            moderationGroup ??= actionsPage.AddElement<GroupElement>("Moderation");

            moderationGroup.AddElement<FunctionElement>("Teleport To Them")
                .WithColor(Color.red)
                .Do(() =>
                {
                    PermissionSender.SendPermissionRequest(PermissionCommandType.TELEPORT_TO_THEM, player);
                });

            moderationGroup.AddElement<FunctionElement>("Teleport To Me")
                .WithColor(Color.red)
                .Do(() =>
                {
                    PermissionSender.SendPermissionRequest(PermissionCommandType.TELEPORT_TO_ME, player);
                });
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

        MortalityElement = generalGroup.AddElement<BoolElement>("Mortality");

        FriendlyFireElement = generalGroup.AddElement<BoolElement>("Friendly Fire");

        KnockoutElement = generalGroup.AddElement<BoolElement>("Knockout");

        KnockoutLengthElement = generalGroup.AddElement<IntElement>("Knockout Length");

        PlayerConstrainingElement = generalGroup.AddElement<BoolElement>("Player Constraining");

        MaxAvatarHeightElement = generalGroup.AddElement<FloatElement>("Max Avatar Height");

        // Permissions
        var permissionGroup = element.AddElement<GroupElement>("Permissions");

        DevToolsElement = permissionGroup.AddElement<EnumElement>("Dev Tools");
        ConstrainerElement = permissionGroup.AddElement<EnumElement>("Constrainer");
        CustomAvatarsElement = permissionGroup.AddElement<EnumElement>("Custom Avatars");
        KickingElement = permissionGroup.AddElement<EnumElement>("Kicking");
        BanningElement = permissionGroup.AddElement<EnumElement>("Banning");
        TeleportationElement = permissionGroup.AddElement<EnumElement>("Teleportation");
    }

    private static void PopulateAdmin(PageElement element)
    {
        var cleanupGroup = element.AddElement<GroupElement>("Cleanup");

        var despawnAllElement = cleanupGroup.AddElement<FunctionElement>("Despawn All")
            .Do(() => PooleeUtilities.DespawnAll());
        var playersGroup = element.AddElement<GroupElement>("Players");

        FusionPermissions.FetchPermissionLevel(PlayerIDManager.LocalPlatformID, out var selfLevel, out _);
        var activeLobbyInfo = LobbyInfoManager.LobbyInfo;

        var teleportAllElement = playersGroup.AddElement<FunctionElement>("Teleport All")
            .Do(() =>
            {
                if (FusionPermissions.HasSufficientPermissions(selfLevel, activeLobbyInfo.Teleportation))
                {
                    foreach (var playerId in PlayerIDManager.PlayerIDs)
                    {
                        if (playerId != PlayerIDManager.LocalSmallID)
                            PermissionSender.SendPermissionRequest(PermissionCommandType.TELEPORT_TO_ME, playerId);
                    }
                }
            });
    }
}