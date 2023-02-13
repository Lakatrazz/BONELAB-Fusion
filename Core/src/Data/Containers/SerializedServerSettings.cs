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
        public FusionPreferences.ServerSettings settings;

        public void Serialize(FusionWriter writer) {
            writer.Write(settings.NametagsEnabled.GetValue());
            writer.Write((byte)settings.Privacy.GetValue());
            writer.Write((byte)settings.TimeScaleMode.GetValue());
            writer.Write(settings.ServerMortality.GetValue());

            writer.Write((byte)settings.DevToolsAllowed.GetValue());
            writer.Write((byte)settings.KickingAllowed.GetValue());
            writer.Write((byte)settings.BanningAllowed.GetValue());
            writer.Write((byte)settings.Teleportation.GetValue());
        }

        public void Deserialize(FusionReader reader) {
            settings = new FusionPreferences.ServerSettings();

            settings.NametagsEnabled = new ReadonlyFusionPrev<bool>(reader.ReadBoolean());
            settings.Privacy = new ReadonlyFusionPrev<ServerPrivacy>((ServerPrivacy)reader.ReadByte());
            settings.TimeScaleMode = new ReadonlyFusionPrev<TimeScaleMode>((TimeScaleMode)reader.ReadByte());
            settings.ServerMortality = new ReadonlyFusionPrev<bool>(reader.ReadBoolean());

            settings.DevToolsAllowed = new ReadonlyFusionPrev<PermissionLevel>((PermissionLevel)reader.ReadByte());
            settings.KickingAllowed = new ReadonlyFusionPrev<PermissionLevel>((PermissionLevel)reader.ReadByte());
            settings.BanningAllowed = new ReadonlyFusionPrev<PermissionLevel>((PermissionLevel)reader.ReadByte());
            settings.Teleportation = new ReadonlyFusionPrev<PermissionLevel>((PermissionLevel)reader.ReadByte());
        }

        public static SerializedServerSettings Create() {
            var settings = new SerializedServerSettings() {
                settings = FusionPreferences.LocalServerSettings,
            };

            return settings;
        }
    }
}
