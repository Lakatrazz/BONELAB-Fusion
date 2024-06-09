using BoneLib.BoneMenu;
using BoneLib.BoneMenu.Elements;
using LabFusion.Data;
using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.Senders;
using MelonLoader;

using UnityEngine;

namespace LabFusion.Preferences
{
    public static class FusionPreferences
    {
        public class ServerSettings
        {
            // General settings
            public IFusionPref<bool> NametagsEnabled;
            public IFusionPref<bool> VoicechatEnabled;
            public IFusionPref<bool> PlayerConstraintsEnabled;
            public IFusionPref<bool> VoteKickingEnabled;
            public IFusionPref<ServerPrivacy> Privacy;
            public IFusionPref<bool> AllowQuestUsers;
            public IFusionPref<bool> AllowPCUsers;
            public IFusionPref<TimeScaleMode> TimeScaleMode;
            public IFusionPref<byte> MaxPlayers;

            // Visual
            public IFusionPref<string> ServerName;
            public IFusionPref<List<string>> ServerTags;

            // Mortality
            public IFusionPref<bool> ServerMortality;

            // Cheat detection
            public IFusionPref<PermissionLevel> StatChangersAllowed;
            public IFusionPref<float> StatChangerLeeway;

            // Permissions
            public IFusionPref<PermissionLevel> DevToolsAllowed;
            public IFusionPref<PermissionLevel> ConstrainerAllowed;
            public IFusionPref<PermissionLevel> CustomAvatarsAllowed;
            public IFusionPref<PermissionLevel> KickingAllowed;
            public IFusionPref<PermissionLevel> BanningAllowed;

            public IFusionPref<PermissionLevel> Teleportation;

            public static ServerSettings CreateMelonPrefs()
            {
                // Server settings
                var settings = new ServerSettings
                {
                    // General settings
                    NametagsEnabled = new FusionPref<bool>(prefCategory, "Server Nametags Enabled", true, PrefUpdateMode.SERVER_UPDATE),
                    VoicechatEnabled = new FusionPref<bool>(prefCategory, "Server Voicechat Enabled", true, PrefUpdateMode.SERVER_UPDATE),
                    PlayerConstraintsEnabled = new FusionPref<bool>(prefCategory, "Server Player Constraints Enabled", false, PrefUpdateMode.SERVER_UPDATE),
                    VoteKickingEnabled = new FusionPref<bool>(prefCategory, "Server Vote Kicking Enabled", true, PrefUpdateMode.SERVER_UPDATE),
                    Privacy = new FusionPref<ServerPrivacy>(prefCategory, "Server Privacy", ServerPrivacy.PUBLIC, PrefUpdateMode.LOCAL_UPDATE),
                    AllowQuestUsers = new FusionPref<bool>(prefCategory, "Allow Quest Users", true, PrefUpdateMode.SERVER_UPDATE),
                    AllowPCUsers = new FusionPref<bool>(prefCategory, "Allow PC Users", true, PrefUpdateMode.SERVER_UPDATE),
                    TimeScaleMode = new FusionPref<TimeScaleMode>(prefCategory, "Time Scale Mode", Senders.TimeScaleMode.LOW_GRAVITY, PrefUpdateMode.SERVER_UPDATE),
                    MaxPlayers = new FusionPref<byte>(prefCategory, "Max Players", 10, PrefUpdateMode.SERVER_UPDATE),

                    // Visual
                    ServerName = new FusionPref<string>(prefCategory, "Server Name", "", PrefUpdateMode.LOCAL_UPDATE),
                    ServerTags = new FusionPref<List<string>>(prefCategory, "Server Tags", new List<string>(), PrefUpdateMode.LOCAL_UPDATE),

                    // Mortality
                    ServerMortality = new FusionPref<bool>(prefCategory, "Server Mortality", true, PrefUpdateMode.SERVER_UPDATE),

                    // Cheat detection
                    StatChangersAllowed = new FusionPref<PermissionLevel>(prefCategory, "Stat Changers Allowed", PermissionLevel.OPERATOR, PrefUpdateMode.SERVER_UPDATE),
                    StatChangerLeeway = new FusionPref<float>(prefCategory, "Stat Changer Leeway", 0f, PrefUpdateMode.SERVER_UPDATE),

                    // Server permissions
                    DevToolsAllowed = new FusionPref<PermissionLevel>(prefCategory, "Dev Tools Allowed", PermissionLevel.DEFAULT, PrefUpdateMode.SERVER_UPDATE),
                    ConstrainerAllowed = new FusionPref<PermissionLevel>(prefCategory, "Constrainer Allowed", PermissionLevel.DEFAULT, PrefUpdateMode.SERVER_UPDATE),
                    CustomAvatarsAllowed = new FusionPref<PermissionLevel>(prefCategory, "Custom Avatars Allowed", PermissionLevel.DEFAULT, PrefUpdateMode.SERVER_UPDATE),
                    KickingAllowed = new FusionPref<PermissionLevel>(prefCategory, "Kicking Allowed", PermissionLevel.OPERATOR, PrefUpdateMode.SERVER_UPDATE),
                    BanningAllowed = new FusionPref<PermissionLevel>(prefCategory, "Banning Allowed", PermissionLevel.OPERATOR, PrefUpdateMode.SERVER_UPDATE),

                    Teleportation = new FusionPref<PermissionLevel>(prefCategory, "Teleportation", PermissionLevel.OPERATOR, PrefUpdateMode.SERVER_UPDATE),
                };

                return settings;
            }
        }

        public struct ClientSettings
        {
            // Selected network layer
            public static FusionPref<string> NetworkLayerTitle { get; internal set; }
            public static FusionPref<int> ProxyPort { get; internal set; }

            // Nametag settings
            public static FusionPref<bool> NametagsEnabled { get; internal set; }
            public static FusionPref<Color> NametagColor { get; internal set; }

            // Nickname settings
            public static FusionPref<string> Nickname { get; internal set; }
            public static FusionPref<NicknameVisibility> NicknameVisibility { get; internal set; }

            // Voicechat settings
            public static FusionPref<bool> Muted { get; internal set; }
            public static FusionPref<bool> MutedIndicator { get; internal set; }
            public static FusionPref<bool> Deafened { get; internal set; }
            public static FusionPref<float> GlobalVolume { get; internal set; }

            // Gamemode settings
            public static FusionPref<bool> GamemodeMusic { get; internal set; }
            public static FusionPref<bool> GamemodeLateJoining { get; internal set; }
        }

        public static ServerSettings LocalServerSettings;
        public static ServerSettings ReceivedServerSettings { get; set; } = null;
        public static ServerSettings ActiveServerSettings => ReceivedServerSettings ?? LocalServerSettings;

        public static bool NametagsEnabled => ActiveServerSettings.NametagsEnabled.GetValue() && ClientSettings.NametagsEnabled;
        public static bool IsMortal => ActiveServerSettings.ServerMortality.GetValue();
        public static TimeScaleMode TimeScaleMode => ActiveServerSettings.TimeScaleMode.GetValue();

        public static MenuCategory fusionCategory;
        public static MelonPreferences_Category prefCategory;

        public static Action OnFusionPreferencesLoaded;

        internal static void SendServerSettings()
        {
            if (NetworkInfo.HasServer && NetworkInfo.IsServer)
            {
                using var writer = FusionWriter.Create();
                var data = ServerSettingsData.Create(SerializedServerSettings.Create());
                writer.Write(data);

                using var message = FusionMessage.Create(NativeMessageTag.ServerSettings, writer);
                MessageSender.BroadcastMessageExceptSelf(NetworkChannel.Reliable, message);
            }
        }

        internal static void SendServerSettings(ulong longId)
        {
            if (NetworkInfo.HasServer && NetworkInfo.IsServer)
            {
                using var writer = FusionWriter.Create(ServerSettingsData.Size);
                var data = ServerSettingsData.Create(SerializedServerSettings.Create());
                writer.Write(data);

                using var message = FusionMessage.Create(NativeMessageTag.ServerSettings, writer);
                MessageSender.SendFromServer(longId, NetworkChannel.Reliable, message);
            }
        }

        internal static void SendClientSettings()
        {
            if (NetworkInfo.HasServer)
            {
                using var writer = FusionWriter.Create(PlayerSettingsData.Size);
                var data = PlayerSettingsData.Create(PlayerIdManager.LocalSmallId, SerializedPlayerSettings.Create());
                writer.Write(data);

                using var message = FusionMessage.Create(NativeMessageTag.PlayerSettings, writer);
                MessageSender.SendToServer(NetworkChannel.Reliable, message);
            }
        }

        internal static void OnInitializePreferences()
        {
            // Create preferences
            prefCategory = MelonPreferences.CreateCategory("BONELAB Fusion");

            // Server settings
            LocalServerSettings = ServerSettings.CreateMelonPrefs();

            // Client settings
            ClientSettings.NetworkLayerTitle = new FusionPref<string>(prefCategory, "Network Layer Title", NetworkLayerDeterminer.GetDefaultLayer().Title, PrefUpdateMode.IGNORE);
            ClientSettings.ProxyPort = new FusionPref<int>(prefCategory, "Proxy Port", 28340, PrefUpdateMode.IGNORE);

            // Nametag
            ClientSettings.NametagsEnabled = new FusionPref<bool>(prefCategory, "Client Nametags Enabled", true, PrefUpdateMode.LOCAL_UPDATE);
            ClientSettings.NametagColor = new FusionPref<Color>(prefCategory, "Nametag Color", Color.white, PrefUpdateMode.CLIENT_UPDATE);

            // Nickname
            ClientSettings.Nickname = new FusionPref<string>(prefCategory, "Nickname", "", PrefUpdateMode.IGNORE);
            ClientSettings.NicknameVisibility = new FusionPref<NicknameVisibility>(prefCategory, "Nickname Visibility", NicknameVisibility.SHOW_WITH_PREFIX, PrefUpdateMode.LOCAL_UPDATE);

            // Voicechat
            ClientSettings.Muted = new FusionPref<bool>(prefCategory, "Muted", false, PrefUpdateMode.IGNORE);
            ClientSettings.MutedIndicator = new FusionPref<bool>(prefCategory, "Muted Indicator", true, PrefUpdateMode.IGNORE);
            ClientSettings.Deafened = new FusionPref<bool>(prefCategory, "Deafened", false, PrefUpdateMode.IGNORE);
            ClientSettings.GlobalVolume = new FusionPref<float>(prefCategory, "GlobalMicVolume", 1f, PrefUpdateMode.IGNORE);

            // Gamemodes
            ClientSettings.GamemodeMusic = new FusionPref<bool>(prefCategory, "Gamemode Music", true, PrefUpdateMode.IGNORE);
            ClientSettings.GamemodeLateJoining = new FusionPref<bool>(prefCategory, "Gamemode Late Joining", true, PrefUpdateMode.IGNORE);

            // Save category
            prefCategory.SaveToFile(false);
        }

        internal static void OnPrepareBoneMenuCategory()
        {
            // We create this here so that its one of the first categories
            fusionCategory = MenuManager.CreateCategory("BONELAB Fusion", Color.white);
        }

        private static int _lastIndex;

        internal static void OnCreateBoneMenu()
        {
            // Make sure the Fusion category is empty
            fusionCategory.Elements.Clear();

            // Create category for changing network layer
            var networkLayerManager = fusionCategory.CreateCategory("Network Layer Manager", Color.yellow);
            networkLayerManager.CreateFunctionElement("Players need to be on the same layer!", Color.yellow, null);

            _lastIndex = NetworkLayer.SupportedLayers.IndexOf(NetworkLayerDeterminer.LoadedLayer);

            networkLayerManager.CreateFunctionElement($"Active Layer: {NetworkLayerDeterminer.LoadedTitle}", Color.white, null);

            var targetPanel = networkLayerManager.CreateSubPanel($"Target Layer: {ClientSettings.NetworkLayerTitle.GetValue()}", Color.white);
            targetPanel.CreateFunctionElement("Cycle", Color.white, () =>
            {
                int count = NetworkLayer.SupportedLayers.Count;
                if (count <= 0)
                    return;

                _lastIndex++;
                if (count <= _lastIndex)
                    _lastIndex = 0;

                ClientSettings.NetworkLayerTitle.SetValue(NetworkLayer.SupportedLayers[_lastIndex].Title);
            });
            ClientSettings.NetworkLayerTitle.OnValueChanged += (v) =>
            {
                targetPanel.SetName($"Target Layer: {v}");
            };

            targetPanel.CreateFunctionElement("SET NETWORK LAYER", Color.green, () => InternalLayerHelpers.UpdateLoadedLayer());

            // Setup bonemenu for the network layer
            InternalLayerHelpers.OnSetupBoneMenuLayer(fusionCategory);
        }

        internal static void OnPreferencesLoaded()
        {
            OnFusionPreferencesLoaded?.Invoke();
        }
    }
}
