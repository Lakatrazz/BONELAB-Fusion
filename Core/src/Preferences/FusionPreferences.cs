using BoneLib.BoneMenu;
using BoneLib.BoneMenu.Elements;
using LabFusion.BoneMenu;
using LabFusion.Data;
using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.Senders;
using MelonLoader;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.Preferences {
    public static class FusionPreferences {
        public class ServerSettings {
            // General settings
            public IFusionPref<bool> NametagsEnabled;
            public IFusionPref<bool> VoicechatEnabled;
            public IFusionPref<ServerPrivacy> Privacy;
            public IFusionPref<TimeScaleMode> TimeScaleMode;
            public IFusionPref<byte> MaxPlayers;

            // Mortality
            public IFusionPref<bool> ServerMortality;

            // Permissions
            public IFusionPref<PermissionLevel> DevToolsAllowed;
            public IFusionPref<PermissionLevel> ConstrainerAllowed;
            public IFusionPref<PermissionLevel> CustomAvatarsAllowed;
            public IFusionPref<PermissionLevel> KickingAllowed;
            public IFusionPref<PermissionLevel> BanningAllowed;

            public IFusionPref<PermissionLevel> Teleportation;

            public static ServerSettings CreateMelonPrefs() {
                // Server settings
                var settings = new ServerSettings
                {
                    // General settings
                    NametagsEnabled = new FusionPref<bool>(prefCategory, "Server Nametags Enabled", true, PrefUpdateMode.SERVER_UPDATE),
                    VoicechatEnabled = new FusionPref<bool>(prefCategory, "Server Voicechat Enabled", true, PrefUpdateMode.SERVER_UPDATE),
                    Privacy = new FusionPref<ServerPrivacy>(prefCategory, "Server Privacy", ServerPrivacy.PUBLIC, PrefUpdateMode.LOCAL_UPDATE),
                    TimeScaleMode = new FusionPref<TimeScaleMode>(prefCategory, "Time Scale Mode", Senders.TimeScaleMode.LOW_GRAVITY, PrefUpdateMode.SERVER_UPDATE),
                    MaxPlayers = new FusionPref<byte>(prefCategory, "Max Players", 10, PrefUpdateMode.SERVER_UPDATE),

                    // Mortality
                    ServerMortality = new FusionPref<bool>(prefCategory, "Server Mortality", true, PrefUpdateMode.SERVER_UPDATE),

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

        public struct ClientSettings {
            // Selected network layer
            public static FusionPref<NetworkLayerType> NetworkLayerType { get; internal set; }
            
            // Nametag settings
            public static FusionPref<bool> NametagsEnabled { get; internal set; }
            public static FusionPref<Color> NametagColor { get; internal set; }

            // Nickname settings
            public static FusionPref<string> Nickname { get; internal set; }
            public static FusionPref<NicknameVisibility> NicknameVisibility { get; internal set; }

            // Voicechat settings
            public static FusionPref<bool> Muted { get; internal set; }
            public static FusionPref<bool> Deafened { get; internal set; }
            public static FusionPref<float> GlobalVolume { get; internal set; }

            // Gamemode settings
            public static FusionPref<bool> GamemodeMusic { get; internal set; }
            public static FusionPref<bool> GamemodeLateJoining { get; internal set; }
        }

        internal static ServerSettings LocalServerSettings;
        internal static ServerSettings ReceivedServerSettings { get; set; } = null;
        internal static ServerSettings ActiveServerSettings => ReceivedServerSettings ?? LocalServerSettings;

        internal static bool NametagsEnabled => ActiveServerSettings.NametagsEnabled.GetValue() && ClientSettings.NametagsEnabled;
        internal static bool IsMortal => ActiveServerSettings.ServerMortality.GetValue();
        internal static TimeScaleMode TimeScaleMode => ActiveServerSettings.TimeScaleMode.GetValue();

        internal static MenuCategory fusionCategory;
        internal static MelonPreferences_Category prefCategory;

        internal static Action OnFusionPreferencesLoaded;

        internal static void SendServerSettings() {
            if (NetworkInfo.HasServer && NetworkInfo.IsServer) {
                using (var writer = FusionWriter.Create()) {
                    using (var data = ServerSettingsData.Create(SerializedServerSettings.Create()))  {
                        writer.Write(data);

                        using (var message = FusionMessage.Create(NativeMessageTag.ServerSettings, writer))
                        {
                            MessageSender.BroadcastMessageExceptSelf(NetworkChannel.Reliable, message);
                        }
                    }
                }
            }
        }

        internal static void SendServerSettings(ulong longId) {
            if (NetworkInfo.HasServer && NetworkInfo.IsServer) {
                using (var writer = FusionWriter.Create(ServerSettingsData.Size))
                {
                    using (var data = ServerSettingsData.Create(SerializedServerSettings.Create()))
                    {
                        writer.Write(data);

                        using (var message = FusionMessage.Create(NativeMessageTag.ServerSettings, writer))
                        {
                            MessageSender.SendFromServer(longId, NetworkChannel.Reliable, message);
                        }
                    }
                }
            }
        }

        internal static void SendClientSettings() {
            if (NetworkInfo.HasServer) {
                using (var writer = FusionWriter.Create(PlayerSettingsData.Size))
                {
                    using (var data = PlayerSettingsData.Create(PlayerIdManager.LocalSmallId, SerializedPlayerSettings.Create()))
                    {
                        writer.Write(data);

                        using (var message = FusionMessage.Create(NativeMessageTag.PlayerSettings, writer))
                        {
                            MessageSender.SendToServer(NetworkChannel.Reliable, message);
                        }
                    }
                }
            }
        }

        internal static void OnInitializePreferences() {
            // Create preferences
            prefCategory = MelonPreferences.CreateCategory("BONELAB Fusion");

            // Server settings
            LocalServerSettings = ServerSettings.CreateMelonPrefs();

            // Client settings
            ClientSettings.NetworkLayerType = new FusionPref<NetworkLayerType>(prefCategory, "Network Layer Type", NetworkLayerDeterminer.GetDefaultType(), PrefUpdateMode.IGNORE);

            // Nametag
            ClientSettings.NametagsEnabled = new FusionPref<bool>(prefCategory, "Client Nametags Enabled", true, PrefUpdateMode.LOCAL_UPDATE);
            ClientSettings.NametagColor = new FusionPref<Color>(prefCategory, "Nametag Color", Color.white, PrefUpdateMode.CLIENT_UPDATE);

            // Nickname
            ClientSettings.Nickname = new FusionPref<string>(prefCategory, "Nickname", "", PrefUpdateMode.IGNORE);
            ClientSettings.NicknameVisibility = new FusionPref<NicknameVisibility>(prefCategory, "Nickname Visibility", NicknameVisibility.SHOW_WITH_PREFIX, PrefUpdateMode.LOCAL_UPDATE);

            // Voicechat
            ClientSettings.Muted = new FusionPref<bool>(prefCategory, "Muted", false, PrefUpdateMode.IGNORE);
            ClientSettings.Deafened = new FusionPref<bool>(prefCategory, "Deafened", false, PrefUpdateMode.IGNORE);
            ClientSettings.GlobalVolume = new FusionPref<float>(prefCategory, "GlobalMicVolume", 1f, PrefUpdateMode.IGNORE);

            // Gamemodes
            ClientSettings.GamemodeMusic = new FusionPref<bool>(prefCategory, "Gamemode Music", true, PrefUpdateMode.IGNORE);
            ClientSettings.GamemodeLateJoining = new FusionPref<bool>(prefCategory, "Gamemode Late Joining", true, PrefUpdateMode.IGNORE);

            // Save category
            prefCategory.SaveToFile(false);
        }

        internal static void OnCreateBoneMenu() {
            // Create BoneMenu
            fusionCategory = MenuManager.CreateCategory("BONELAB Fusion", Color.white);

            // Create category for changing network layer
            var networkLayerManager = fusionCategory.CreateCategory("Network Layer Manager", Color.yellow);
            networkLayerManager.CreateFunctionElement("Players need to be on the same layer!", Color.yellow, null);

            networkLayerManager.CreateFunctionElement($"Active Layer: {NetworkLayerDeterminer.LoadedType}", Color.white, null);
            BoneMenuCreator.CreateEnumPreference(networkLayerManager, "Target Layer", ClientSettings.NetworkLayerType);
            networkLayerManager.CreateFunctionElement("Changing this setting requires a game restart!", Color.red, null);

            // Setup bonemenu for the network layer
            InternalLayerHelpers.OnSetupBoneMenuLayer(fusionCategory);
        }

        internal static void OnPreferencesLoaded() {
            OnFusionPreferencesLoaded?.Invoke();
        }
    }
}
