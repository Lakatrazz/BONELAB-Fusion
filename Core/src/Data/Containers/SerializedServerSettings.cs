using LabFusion.Network;
using LabFusion.Utilities;
using LabFusion.Preferences;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.Data {
    public class SerializedServerSettings : IFusionSerializable {
        public bool nametagsEnabled;

        public void Serialize(FusionWriter writer) {
            writer.Write(nametagsEnabled);
        }

        public void Deserialize(FusionReader reader) {
            nametagsEnabled = reader.ReadBoolean();
        }

        public static SerializedServerSettings Create() {
            var settings = new SerializedServerSettings() {
                nametagsEnabled = FusionPreferences.ServerSettings.NametagsEnabled,
            };

            return settings;
        }
    }
}
