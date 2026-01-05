using Il2CppSLZ.Marrow.Warehouse;

using LabFusion.Data;
using LabFusion.Entities;
using LabFusion.Marrow;
using LabFusion.Marrow.Serialization;
using LabFusion.Safety;
using LabFusion.Utilities;

namespace LabFusion.Network;

[Net.DelayWhileTargetLoading]
public class SpawnRequestMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.SpawnRequest;

    public override ExpectedReceiverType ExpectedReceiver => ExpectedReceiverType.ServerOnly;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<SerializedSpawnData>();

        // Check for spawnable blacklist
        if (ModBlacklist.IsBlacklisted(data.Barcode) || GlobalModBlacklistManager.IsBarcodeBlacklisted(data.Barcode))
        {
#if DEBUG
            FusionLogger.Warn($"Blocking server spawn of spawnable {data.Barcode} because it is blacklisted!");
#endif

            return;
        }

        // Check for singleplayer only tag
        if (CrateFilterer.HasTags<SpawnableCrate>(new(data.Barcode), FusionTags.SingleplayerOnly))
        {
#if DEBUG
            FusionLogger.Warn($"Blocking server spawn of spawnable {data.Barcode} because it is tagged Singleplayer Only!");
#endif

            return;
        }

        var entityID = NetworkEntityManager.IDManager.RegisteredEntities.AllocateNewID();

        var responseData = new SpawnResponseData()
        {
            OwnerID = received.Sender.Value,
            EntityID = entityID,
            SpawnData = data,
        };

        MessageRelay.RelayNative(responseData, NativeMessageTag.SpawnResponse, CommonMessageRoutes.ReliableToClients);
    }
}