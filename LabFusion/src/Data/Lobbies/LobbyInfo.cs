using LabFusion.Marrow;
using LabFusion.Network;
using LabFusion.Player;
using LabFusion.Preferences.Server;
using LabFusion.Representation;
using LabFusion.Scene;
using LabFusion.SDK.Gamemodes;
using LabFusion.Senders;

using System.Text.Json.Serialization;

namespace LabFusion.Data;

[Serializable]
public class LobbyInfo
{
    public static readonly LobbyInfo Empty = new();

    // Info
    [JsonPropertyName("lobbyID")]
    public string LobbyID { get; set; } = "";

    [JsonPropertyName("lobbyCode")]
    public string LobbyCode { get; set; } = null;

    [JsonPropertyName("lobbyName")]
    public string LobbyName { get; set; } = null;

    [JsonPropertyName("lobbyDescription")]
    public string LobbyDescription { get; set; } = null;

    [JsonPropertyName("lobbyVersion")]
    public Version LobbyVersion { get; set; } = new();

    [JsonPropertyName("lobbyHostName")]
    public string LobbyHostName { get; set; } = null;

    [JsonPropertyName("playerCount")]
    public int PlayerCount { get; set; } = 0;

    [JsonPropertyName("playerList")]
    public PlayerList PlayerList { get; set; } = new();

    // Location
    [JsonPropertyName("levelTitle")]
    public string LevelTitle { get; set; } = null;

    [JsonPropertyName("levelBarcode")]
    public string LevelBarcode { get; set; } = null;

    [JsonPropertyName("levelModID")]
    public int LevelModID { get; set; } = -1;

    // Gamemode
    [JsonPropertyName("gamemodeTitle")]
    public string GamemodeTitle { get; set; } = null;

    [JsonPropertyName("gamemodeBarcode")]
    public string GamemodeBarcode { get; set; } = null;

    [JsonPropertyName("timeBetweenGamemodeRounds")]
    public int TimeBetweenGamemodeRounds { get; set; } = 30;

    // Settings
    [JsonPropertyName("nameTags")]
    public bool NameTags { get; set; } = false;

    [JsonPropertyName("privacy")]
    public ServerPrivacy Privacy { get; set; } = ServerPrivacy.PUBLIC;

    [JsonPropertyName("slowMoMode")]
    public TimeScaleMode SlowMoMode { get; set; } = TimeScaleMode.DISABLED;

    [JsonPropertyName("maxPlayers")]
    public int MaxPlayers { get; set; } = 0;

    [JsonPropertyName("voiceChat")]
    public bool VoiceChat { get; set; } = false;

    [JsonPropertyName("playerConstraining")]
    public bool PlayerConstraining { get; set; } = false;

    [JsonPropertyName("mortality")]
    public bool Mortality { get; set; } = false;

    [JsonPropertyName("friendlyFire")]
    public bool FriendlyFire { get; set; } = false;

    [JsonPropertyName("knockout")]
    public bool Knockout { get; set; } = false;

    [JsonPropertyName("knockoutLength")]
    public int KnockoutLength { get; set; } = 0;

    [JsonPropertyName("maxAvatarHeight")]
    public float MaxAvatarHeight { get; set; } = 0f;

    // Permissions
    [JsonPropertyName("devTools")]
    public PermissionLevel DevTools { get; set; } = PermissionLevel.DEFAULT;

    [JsonPropertyName("constrainer")]
    public PermissionLevel Constrainer { get; set; } = PermissionLevel.DEFAULT;

    [JsonPropertyName("customAvatars")]
    public PermissionLevel CustomAvatars { get; set; } = PermissionLevel.DEFAULT;

    [JsonPropertyName("kicking")]
    public PermissionLevel Kicking { get; set; } = PermissionLevel.DEFAULT;

    [JsonPropertyName("banning")]
    public PermissionLevel Banning { get; set; } = PermissionLevel.DEFAULT;

    [JsonPropertyName("teleportation")]
    public PermissionLevel Teleportation { get; set; } = PermissionLevel.DEFAULT;

    public void WriteLobby()
    {
        // Info
        LobbyID = PlayerIDManager.LocalPlatformID;
        LobbyCode = NetworkHelper.GetServerCode();
        LobbyName = SavedServerSettings.ServerName.Value;
        LobbyDescription = SavedServerSettings.ServerDescription.Value;
        LobbyVersion = FusionMod.Version;
        LobbyHostName = LocalPlayer.Username;

        PlayerCount = PlayerIDManager.PlayerCount;

        var playerList = new PlayerList();
        playerList.WritePlayers();
        PlayerList = playerList;

        // Location
        LevelTitle = FusionSceneManager.Title;
        LevelBarcode = FusionSceneManager.Barcode;

        LevelModID = CrateFilterer.GetModID(FusionSceneManager.Level.Pallet);

        // Gamemode
        GamemodeTitle = string.Empty;
        GamemodeBarcode = string.Empty;

        if (GamemodeManager.ActiveGamemode != null)
        {
            GamemodeTitle = GamemodeManager.ActiveGamemode.Title;
            GamemodeBarcode = GamemodeManager.ActiveGamemode.Barcode;
        }

        TimeBetweenGamemodeRounds = GamemodeRoundManager.Settings.TimeBetweenRounds;

        // Settings
        NameTags = SavedServerSettings.NameTags.Value;
        Privacy = SavedServerSettings.Privacy.Value;
        SlowMoMode = SavedServerSettings.SlowMoMode.Value;
        MaxPlayers = SavedServerSettings.MaxPlayers.Value;
        VoiceChat = SavedServerSettings.VoiceChat.Value;
        PlayerConstraining = SavedServerSettings.PlayerConstraining.Value;
        Mortality = SavedServerSettings.Mortality.Value;
        FriendlyFire = SavedServerSettings.FriendlyFire.Value;
        Knockout = SavedServerSettings.Knockout.Value;
        KnockoutLength = SavedServerSettings.KnockoutLength.Value;
        MaxAvatarHeight = SavedServerSettings.MaxAvatarHeight.Value;

        // Permissions
        DevTools = SavedServerSettings.DevTools.Value;
        Constrainer = SavedServerSettings.Constrainer.Value;
        CustomAvatars = SavedServerSettings.CustomAvatars.Value;
        Kicking = SavedServerSettings.Kicking.Value;
        Banning = SavedServerSettings.Banning.Value;
        Teleportation = SavedServerSettings.Teleportation.Value;
    }
}