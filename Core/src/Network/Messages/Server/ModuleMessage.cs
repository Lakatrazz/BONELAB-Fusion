using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LabFusion.Data;
using LabFusion.Exceptions;
using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.Utilities;

namespace LabFusion.Network {
    public class ModuleMessage : FusionMessageHandler {
        public override byte? Tag => NativeMessageTag.Module;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false) {
            ModuleMessageHandler.ReadMessage(bytes, isServerHandled);
        }
    }
}
