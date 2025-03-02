using LabFusion.Data;
using LabFusion.Network.Serialization;
using LabFusion.Patching;

namespace LabFusion.Network;

public class GeoSelectData : INetSerializable
{
    public byte geoIndex;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref geoIndex);
    }

    public static GeoSelectData Create(byte geoIndex)
    {
        return new GeoSelectData()
        {
            geoIndex = geoIndex,
        };
    }
}

[Net.DelayWhileTargetLoading]
public class GeoSelectMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.GeoSelect;

    public override ExpectedReceiverType ExpectedReceiver => ExpectedReceiverType.ClientsOnly;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<GeoSelectData>();
        var manager = ArenaData.GeoManager;

        if (!manager)
        {
            return;
        }

        GeoManagerPatches.IgnorePatches = true;

        manager.ClearCurrentGeo();
        manager.ToggleGeo(data.geoIndex);

        GeoManagerPatches.IgnorePatches = false;
    }
}
