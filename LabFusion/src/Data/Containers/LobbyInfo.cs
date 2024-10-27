using LabFusion.Network;
using LabFusion.Player;
using LabFusion.Preferences.Server;
using LabFusion.Representation;
using LabFusion.Scene;
using LabFusion.Senders;

using System.Text.Json.Serialization;

namespace LabFusion.Data;

[Serializable]
public class LobbyInfo
{
    // Info
    [JsonPropertyName("lobbyId")]
    public ulong LobbyId { get; set; }

    [JsonPropertyName("lobbyCode")]
    public string LobbyCode { get; set; }

    [JsonPropertyName("lobbyName")]
    public string LobbyName { get; set; }

    [JsonPropertyName("lobbyDescription")]
    public string LobbyDescription { get; set; }

    [JsonPropertyName("lobbyVersion")]
    public Version LobbyVersion { get; set; }

    [JsonPropertyName("lobbyHostName")]
    public string LobbyHostName { get; set; }

    [JsonPropertyName("playerCount")]
    public int PlayerCount { get; set; }

    [JsonPropertyName("playerList")]
    public PlayerList PlayerList { get; set; }

    // Location
    [JsonPropertyName("levelTitle")]
    public string LevelTitle { get; set; }

    [JsonPropertyName("levelBarcode")]
    public string LevelBarcode { get; set; }

    [JsonPropertyName("levelModId")]
    public int LevelModId { get; set; } = -1;

    // Settings
    [JsonPropertyName("nameTags")]
    public bool NameTags { get; set; }

    [JsonPropertyName("privacy")]
    public ServerPrivacy Privacy { get; set; }

    [JsonPropertyName("slowMoMode")]
    public TimeScaleMode SlowMoMode { get; set; }

    [JsonPropertyName("maxPlayers")]
    public int MaxPlayers { get; set; }

    [JsonPropertyName("voiceChat")]
    public bool VoiceChat { get; set; }

    [JsonPropertyName("playerConstraints")]
    public bool PlayerConstraints { get; set; }

    [JsonPropertyName("mortality")]
    public bool Mortality { get; set; }

    // Permissions
    [JsonPropertyName("devTools")]
    public PermissionLevel DevTools { get; set; }

    [JsonPropertyName("constrainer")]
    public PermissionLevel Constrainer { get; set; }

    [JsonPropertyName("customAvatars")]
    public PermissionLevel CustomAvatars { get; set; }

    [JsonPropertyName("kicking")]
    public PermissionLevel Kicking { get; set; }

    [JsonPropertyName("banning")]
    public PermissionLevel Banning { get; set; }

    [JsonPropertyName("teleportation")]
    public PermissionLevel Teleportation { get; set; }

    public void WriteLobby()
    {
        // Info
        LobbyId = PlayerIdManager.LocalLongId;
        LobbyCode = NetworkHelper.GetServerCode();
        LobbyName = SavedServerSettings.ServerName.Value;
        LobbyDescription = SavedServerSettings.ServerDescription.Value;
        LobbyVersion = FusionMod.Version;
        LobbyHostName = PlayerIdManager.LocalUsername;

        PlayerCount = PlayerIdManager.PlayerCount;

        var playerList = new PlayerList();
        playerList.WritePlayers();
        PlayerList = playerList;

        // Location
        LevelTitle = FusionSceneManager.Title;
        LevelBarcode = FusionSceneManager.Barcode;

        // Settings
        NameTags = SavedServerSettings.NameTags.Value;
        Privacy = SavedServerSettings.Privacy.Value;
        SlowMoMode = SavedServerSettings.SlowMoMode.Value;
        MaxPlayers = SavedServerSettings.MaxPlayers.Value;
        VoiceChat = SavedServerSettings.VoiceChat.Value;
        PlayerConstraints = SavedServerSettings.PlayerConstraints.Value;
        Mortality = SavedServerSettings.Mortality.Value;

        // Permissions
        DevTools = SavedServerSettings.DevTools.Value;
        Constrainer = SavedServerSettings.Constrainer.Value;
        CustomAvatars = SavedServerSettings.CustomAvatars.Value;
        Kicking = SavedServerSettings.Kicking.Value;
        Banning = SavedServerSettings.Banning.Value;
        Teleportation = SavedServerSettings.Teleportation.Value;
    }
}