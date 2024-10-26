using LabFusion.Network;
using LabFusion.Player;
using LabFusion.Preferences.Server;
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
    public int LevelModId { get; set; }

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

    public void WriteLobby()
    {
        // Info
        LobbyId = PlayerIdManager.LocalLongId;
        LobbyCode = NetworkHelper.GetServerCode();
        LobbyName = ServerSettingsManager.SavedSettings.ServerName.Value;
        LobbyDescription = ServerSettingsManager.SavedSettings.ServerDescription.Value;
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
        NameTags = ServerSettingsManager.SavedSettings.NametagsEnabled.Value;
        Privacy = ServerSettingsManager.SavedSettings.Privacy.Value;
        SlowMoMode = ServerSettingsManager.SavedSettings.TimeScaleMode.Value;
        MaxPlayers = ServerSettingsManager.SavedSettings.MaxPlayers.Value;
        VoiceChat = ServerSettingsManager.SavedSettings.VoiceChatEnabled.Value;
    }
}