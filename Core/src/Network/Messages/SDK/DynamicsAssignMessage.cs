using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LabFusion.Data;
using LabFusion.Exceptions;
using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.SDK.Gamemodes;
using LabFusion.Utilities;

namespace LabFusion.Network
{
    public class DynamicsAssignData : IFusionSerializable, IDisposable
    {
        public string[] moduleHandlerNames;
        public string[] gamemodeNames;
        public Dictionary<string, string>[] gamemodeMetadatas;

        public void Serialize(FusionWriter writer)
        {
            // Write module and gamemode names
            writer.Write(moduleHandlerNames);
            writer.Write(gamemodeNames);

            // Write the length of metadata
            int length = gamemodeMetadatas.Length;
            writer.Write(length);

            // Write all metadata
            for (var i = 0; i < length; i++) {
                writer.Write(gamemodeMetadatas[i]);
            }
        }

        public void Deserialize(FusionReader reader)
        {
            // Read the module and gamemode names
            moduleHandlerNames = reader.ReadStrings();
            gamemodeNames = reader.ReadStrings();

            // Read the length of metadata
            int length = reader.ReadInt32();

            // Read all active metadata info
            gamemodeMetadatas = new Dictionary<string, string>[length];
            for (var i = 0; i < length; i++) {
                gamemodeMetadatas[i] = reader.ReadStringDictionary();
            }
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public static DynamicsAssignData Create() {
            return new DynamicsAssignData() {
                moduleHandlerNames = ModuleMessageHandler.GetExistingTypeNames(),
                gamemodeNames = GamemodeRegistration.GetExistingTypeNames(),
                gamemodeMetadatas = GamemodeRegistration.GetExistingMetadata(),
            };
        }
    }

    public class DynamicsAssignMessage : FusionMessageHandler
    {
        public override byte? Tag => NativeMessageTag.DynamicsAssignment;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
        {
            if (NetworkInfo.IsServer || isServerHandled)
                throw new ExpectedClientException();

            using (FusionReader reader = FusionReader.Create(bytes)) {
                using (var data = reader.ReadFusionSerializable<DynamicsAssignData>()) {
                    // Modules
                    ModuleMessageHandler.PopulateHandlerTable(data.moduleHandlerNames);

                    // Gamemodes
                    GamemodeRegistration.PopulateGamemodeTable(data.gamemodeNames);
                    GamemodeRegistration.PopulateGamemodeMetadatas(data.gamemodeMetadatas);
                }
            }
        }
    }
}
