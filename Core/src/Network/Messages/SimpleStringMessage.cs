using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LabFusion.Data;
using LabFusion.Utilities;

namespace LabFusion.Network
{
    public class SimpleStringMessage : FusionMessageHandler {
        public override byte? Tag => NativeMessageTag.Unknown;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false) {
            using (FusionReader reader = FusionReader.Create(bytes)) {
                var str = reader.ReadString();
                FusionLogger.Log($"Received text: {str}");
            }
        }
    }
}
