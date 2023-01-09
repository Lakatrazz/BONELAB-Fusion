using BoneLib.BoneMenu;
using BoneLib.BoneMenu.Elements;
using LabFusion.Data;
using LabFusion.Network;
using LabFusion.Representation;

using MelonLoader;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.Utilities {
    internal static class FusionPreferences {
        internal struct ServerSettings  {
            public static FusionPref<bool> NametagsEnabled { get; set; }
        }

        internal struct ClientSettings {
            public static FusionPref<bool> NametagsEnabled { get; set; }
            public static FusionPref<Color> NametagColor { get; set; }
        }

        internal static SerializedServerSettings ReceivedServerSettings { get; set; } = null;

        internal static bool ShowNametags => ReceivedServerSettings != null ? ReceivedServerSettings.nametagsEnabled : ServerSettings.NametagsEnabled 
            && ClientSettings.NametagsEnabled;

        internal static MenuCategory fusionCategory;
        internal static MelonPreferences_Category prefCategory;

        internal static Action OnFusionPreferencesLoaded;

        internal static Action OnServerSettingsChange;

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
                using (var writer = FusionWriter.Create())
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
                using (var writer = FusionWriter.Create())
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
            ServerSettings.NametagsEnabled = new FusionPref<bool>(prefCategory, "Server Nametags Enabled", true, PrefUpdateMode.SERVER_UPDATE);

            // Client settings
            ClientSettings.NametagsEnabled = new FusionPref<bool>(prefCategory, "Client Nametags Enabled", true, PrefUpdateMode.LOCAL_UPDATE);
            ClientSettings.NametagColor = new FusionPref<Color>(prefCategory, "Nametag Color", Color.white, PrefUpdateMode.CLIENT_UPDATE);

            // Save category
            prefCategory.SaveToFile(false);

            // Create BoneMenu
            fusionCategory = MenuManager.CreateCategory("BONELAB Fusion", Color.white);

            InternalLayerHelpers.OnSetupBoneMenuLayer(fusionCategory);
        }

        internal static void OnPreferencesLoaded() {
            OnFusionPreferencesLoaded?.Invoke();
        }
    }
}
