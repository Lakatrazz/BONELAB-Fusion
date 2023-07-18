using LabFusion.Network;
using LabFusion.Utilities;
using LabFusion.Preferences;
using LabFusion.Senders;
using LabFusion.Representation;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.Data {
    public class SerializedServerSettings : IFusionSerializable {
        public const int Size = sizeof(byte) * 13;

        public FusionPreferences.ServerSettings settings;

        public void Serialize(FusionWriter writer) {
            // General settings
            writer.Write(settings.NametagsEnabled.GetValue());
            writer.Write(settings.VoicechatEnabled.GetValue());
            writer.Write(settings.PlayerConstraintsEnabled.GetValue());
            writer.Write(settings.VoteKickingEnabled.GetValue());
            writer.Write((byte)settings.Privacy.GetValue());
            writer.Write((byte)settings.TimeScaleMode.GetValue());
            writer.Write(settings.MaxPlayers.GetValue());

            // Visual
            writer.Write(settings.ServerName.GetValue());
            writer.Write(settings.ServerTags.GetValue());

            // Mortality
            writer.Write(settings.ServerMortality.GetValue());

            // Server permissions
            writer.Write((byte)settings.DevToolsAllowed.GetValue());
            writer.Write((byte)settings.ConstrainerAllowed.GetValue());
            writer.Write((byte)settings.CustomAvatarsAllowed.GetValue());
            writer.Write((byte)settings.KickingAllowed.GetValue());
            writer.Write((byte)settings.BanningAllowed.GetValue());
            writer.Write((byte)settings.Teleportation.GetValue());
        }

        public void Deserialize(FusionReader reader) {
            settings = new FusionPreferences.ServerSettings
            {
                // General settings
                NametagsEnabled = new ReadonlyFusionPrev<bool>(reader.ReadBoolean()),
                VoicechatEnabled = new ReadonlyFusionPrev<bool>(reader.ReadBoolean()),
                PlayerConstraintsEnabled = new ReadonlyFusionPrev<bool>(reader.ReadBoolean()),
                VoteKickingEnabled = new ReadonlyFusionPrev<bool>(reader.ReadBoolean()),
                Privacy = new ReadonlyFusionPrev<ServerPrivacy>((ServerPrivacy)reader.ReadByte()),
                TimeScaleMode = new ReadonlyFusionPrev<TimeScaleMode>((TimeScaleMode)reader.ReadByte()),
                MaxPlayers = new ReadonlyFusionPrev<byte>(reader.ReadByte()),

                // Visual
                ServerName = new ReadonlyFusionPrev<string>(reader.ReadString()),
                ServerTags = new ReadonlyFusionPrev<List<string>>(reader.ReadStrings().ToList()),

                // Mortality
                ServerMortality = new ReadonlyFusionPrev<bool>(reader.ReadBoolean()),

                // Server permissions
                DevToolsAllowed = new ReadonlyFusionPrev<PermissionLevel>((PermissionLevel)reader.ReadByte()),
                ConstrainerAllowed = new ReadonlyFusionPrev<PermissionLevel>((PermissionLevel)reader.ReadByte()),
                CustomAvatarsAllowed = new ReadonlyFusionPrev<PermissionLevel>((PermissionLevel)reader.ReadByte()),
                KickingAllowed = new ReadonlyFusionPrev<PermissionLevel>((PermissionLevel)reader.ReadByte()),
                BanningAllowed = new ReadonlyFusionPrev<PermissionLevel>((PermissionLevel)reader.ReadByte()),
                Teleportation = new ReadonlyFusionPrev<PermissionLevel>((PermissionLevel)reader.ReadByte()),
            };
        }

        public static SerializedServerSettings Create() {
            var settings = new SerializedServerSettings() {
                settings = FusionPreferences.LocalServerSettings,
            };

            return settings;
        }
    }
}
