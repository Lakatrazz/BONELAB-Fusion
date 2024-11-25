using LabFusion.Data;
using LabFusion.Exceptions;
using LabFusion.SDK.Gamemodes;
using LabFusion.SDK.Modules;

namespace LabFusion.Network;

public class DynamicsAssignData : IFusionSerializable
{
    public string[] moduleHandlerNames;
    public Dictionary<string, Dictionary<string, string>> gamemodeMetadatas;

    public void Serialize(FusionWriter writer)
    {
        // Write module names
        writer.Write(moduleHandlerNames);

        // Write the length of metadata
        int length = gamemodeMetadatas.Count;
        writer.Write(length);

        // Write all metadata
        foreach (var pair in gamemodeMetadatas)
        {
            var barcode = pair.Key;
            var metadata = pair.Value;

            writer.Write(barcode);
            writer.Write(metadata);
        }
    }

    public void Deserialize(FusionReader reader)
    {
        // Read the module and gamemode names
        moduleHandlerNames = reader.ReadStrings();

        // Read the length of metadata
        int length = reader.ReadInt32();

        // Read all active metadata info
        gamemodeMetadatas = new Dictionary<string, Dictionary<string, string>>();
        for (var i = 0; i < length; i++)
        {
            var barcode = reader.ReadString();
            var metadata = reader.ReadStringDictionary();

            gamemodeMetadatas.Add(barcode, metadata);
        }
    }

    public static DynamicsAssignData Create()
    {
        return new DynamicsAssignData()
        {
            moduleHandlerNames = ModuleMessageHandler.GetExistingTypeNames(),
            gamemodeMetadatas = GamemodeRegistration.GetExistingMetadata(),
        };
    }
}

public class DynamicsAssignMessage : FusionMessageHandler
{
    public override byte Tag => NativeMessageTag.DynamicsAssignment;

    public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
    {
        if (NetworkInfo.IsServer || isServerHandled)
        {
            throw new ExpectedClientException();
        }

        using FusionReader reader = FusionReader.Create(bytes);
        var data = reader.ReadFusionSerializable<DynamicsAssignData>();

        // Modules
        ModuleMessageHandler.PopulateHandlerTable(data.moduleHandlerNames);

        // Gamemodes
        GamemodeRegistration.PopulateGamemodeMetadatas(data.gamemodeMetadatas);
    }
}