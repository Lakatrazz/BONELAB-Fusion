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

namespace LabFusion.Network
{
    public class ModuleAssignData : IFusionSerializable, IDisposable
    {
        public string[] handlerNames;

        public void Serialize(FusionWriter writer)
        {
            writer.Write(handlerNames);
        }

        public void Deserialize(FusionReader reader)
        {
            handlerNames = reader.ReadStrings();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public static ModuleAssignData Create() {
            return new ModuleAssignData() {
                handlerNames = ModuleMessageHandler.GetExistingTypeNames()
            };
        }
    }

    public class ModuleAssignMessage : FusionMessageHandler
    {
        public override byte? Tag => NativeMessageTag.ModuleAssignment;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
        {
            if (NetworkInfo.IsServer || isServerHandled)
                throw new ExpectedClientException();

            using (FusionReader reader = FusionReader.Create(bytes)) {
                using (var data = reader.ReadFusionSerializable<ModuleAssignData>()) {
                    ModuleMessageHandler.PopulateHandlerTable(data.handlerNames);
                }
            }
        }
    }
}
