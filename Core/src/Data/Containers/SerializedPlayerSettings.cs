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
    public class SerializedPlayerSettings : IFusionSerializable {
        public const int Size = sizeof(float) * 4;

        public Color nametagColor;

        public void Serialize(FusionWriter writer) {
            writer.Write(nametagColor);
        }

        public void Deserialize(FusionReader reader) {
            nametagColor = reader.ReadColor();
        }

        public static SerializedPlayerSettings Create() {
            var settings = new SerializedPlayerSettings() {
                nametagColor = FusionPreferences.ClientSettings.NametagColor
            };

            return settings;
        }
    }
}
