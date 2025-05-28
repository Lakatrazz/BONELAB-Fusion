using LabFusion.Extensions;
using LabFusion.Network.Serialization;

using LabFusion.SDK.Gamemodes;
using LabFusion.SDK.Modules;

namespace LabFusion.Network;

public class DynamicsAssignData : INetSerializable
{
    public string[] ModuleHandlerNames;

    public Dictionary<string, Dictionary<string, string>> GamemodeMetadatas;

    public int? GetSize()
    {
        int size = 0;

        size += sizeof(int);
        foreach (var name in ModuleHandlerNames)
        {
            size += name.GetSize();
        }

        size += sizeof(int);
        foreach (var metadata in GamemodeMetadatas)
        {
            size += metadata.Key.GetSize();

            size += sizeof(int);
            foreach (var pair in metadata.Value)
            {
                size += pair.Key.GetSize();
                size += pair.Value.GetSize();
            }
        }

        return size;
    }

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref ModuleHandlerNames);

        if (serializer.IsReader)
        {
            int length = 0;
            serializer.SerializeValue(ref length);

            GamemodeMetadatas = new(length);

            for (var i = 0; i < length; i++)
            {
                string barcode = null;
                Dictionary<string, string> metadata = null;

                serializer.SerializeValue(ref barcode);
                serializer.SerializeValue(ref metadata);

                GamemodeMetadatas.Add(barcode, metadata);
            }
        }
        else
        {
            int length = GamemodeMetadatas.Count;
            serializer.SerializeValue(ref length);

            foreach (var pair in GamemodeMetadatas)
            {
                var barcode = pair.Key;
                var metadata = pair.Value;

                serializer.SerializeValue(ref barcode);
                serializer.SerializeValue(ref metadata);
            }
        }
    }

    public static DynamicsAssignData Create()
    {
        return new DynamicsAssignData()
        {
            ModuleHandlerNames = ModuleMessageHandler.GetExistingTypeNames(),
            GamemodeMetadatas = GamemodeRegistration.GetExistingMetadata(),
        };
    }
}

public class DynamicsAssignMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.DynamicsAssignment;

    public override ExpectedReceiverType ExpectedReceiver => ExpectedReceiverType.ClientsOnly;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<DynamicsAssignData>();

        // Modules
        ModuleMessageHandler.PopulateHandlerTable(data.ModuleHandlerNames);

        // Gamemodes
        GamemodeRegistration.PopulateGamemodeMetadatas(data.GamemodeMetadatas);
    }
}