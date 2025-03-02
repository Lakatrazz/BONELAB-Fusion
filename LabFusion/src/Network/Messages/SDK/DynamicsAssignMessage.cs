using LabFusion.Network.Serialization;

using LabFusion.SDK.Gamemodes;
using LabFusion.SDK.Modules;

namespace LabFusion.Network;

public class DynamicsAssignData : INetSerializable
{
    public string[] moduleHandlerNames;
    public Dictionary<string, Dictionary<string, string>> gamemodeMetadatas;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref moduleHandlerNames);

        if (serializer.IsReader)
        {
            int length = 0;
            serializer.SerializeValue(ref length);

            gamemodeMetadatas = new(length);

            for (var i = 0; i < length; i++)
            {
                string barcode = null;
                Dictionary<string, string> metadata = null;

                serializer.SerializeValue(ref barcode);
                serializer.SerializeValue(ref metadata);

                gamemodeMetadatas.Add(barcode, metadata);
            }
        }
        else
        {
            int length = gamemodeMetadatas.Count;
            serializer.SerializeValue(ref length);

            foreach (var pair in gamemodeMetadatas)
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
            moduleHandlerNames = ModuleMessageHandler.GetExistingTypeNames(),
            gamemodeMetadatas = GamemodeRegistration.GetExistingMetadata(),
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
        ModuleMessageHandler.PopulateHandlerTable(data.moduleHandlerNames);

        // Gamemodes
        GamemodeRegistration.PopulateGamemodeMetadatas(data.gamemodeMetadatas);
    }
}