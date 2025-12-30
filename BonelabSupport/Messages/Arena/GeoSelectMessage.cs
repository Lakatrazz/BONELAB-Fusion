using MarrowFusion.Bonelab.Scene;
using MarrowFusion.Bonelab.Patching;

using LabFusion.Network.Serialization;
using LabFusion.Network;
using LabFusion.SDK.Modules;

namespace MarrowFusion.Bonelab.Messages;

public class GeoSelectData : INetSerializable
{
    public byte GeoIndex;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref GeoIndex);
    }
}

[Net.DelayWhileTargetLoading]
public class GeoSelectMessage : ModuleMessageHandler
{
    public override ExpectedReceiverType ExpectedReceiver => ExpectedReceiverType.ClientsOnly;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<GeoSelectData>();

        var manager = ArenaEventHandler.GeoManager;

        if (!manager)
        {
            return;
        }

        GeoManagerPatches.IgnorePatches = true;

        try
        {
            manager.ClearCurrentGeo();
            manager.ToggleGeo(data.GeoIndex);
        }
        catch (Exception e)
        {
            BonelabModule.Logger.LogException("handling GeoSelectMessage", e);
        }

        GeoManagerPatches.IgnorePatches = false;
    }
}
