using LabFusion.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Network
{
    public class SimpleStringMessage : FusionMessageHandler {
        public override void HandleMessage(byte[] bytes) {
            using (FusionReader reader = FusionReader.Create(bytes)) {
                var str = reader.ReadString();
                FusionLogger.Log($"Received text: {str}");
            }
        }
    }
}
