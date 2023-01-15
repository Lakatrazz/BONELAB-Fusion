using LabFusion.Network;
using LabFusion.Utilities;
using LabFusion.Preferences;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

using LabFusion.Senders;

namespace LabFusion.Data {
    public class SerializedServerSettings : IFusionSerializable {
        public bool nametagsEnabled;
        public TimeScaleMode timeScaleMode;

        public void Serialize(FusionWriter writer) {
            writer.Write(nametagsEnabled);
            writer.Write((byte)timeScaleMode);
        }

        public void Deserialize(FusionReader reader) {
            nametagsEnabled = reader.ReadBoolean();
            timeScaleMode = (TimeScaleMode)reader.ReadByte();
        }

        public static SerializedServerSettings Create() {
            var settings = new SerializedServerSettings() {
                nametagsEnabled = FusionPreferences.ServerSettings.NametagsEnabled,
                timeScaleMode = FusionPreferences.ServerSettings.TimeScaleMode,
            };

            return settings;
        }
    }
}
