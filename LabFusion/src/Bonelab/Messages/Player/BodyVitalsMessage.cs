using LabFusion.Entities;
using LabFusion.Network;
using LabFusion.Bonelab.Data;
using LabFusion.Bonelab.Extenders;
using LabFusion.SDK.Modules;

namespace LabFusion.Bonelab.Messages;

public class BodyVitalsMessage : ModuleMessageHandler
{
    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var bodyVitals = received.ReadData<SerializedBodyVitals>();

        var sender = received.Sender;

        if (!sender.HasValue)
        {
            return;
        }

        if (!NetworkPlayerManager.TryGetPlayer(sender.Value, out var player))
        {
            return;
        }

        var bonelabPlayer = player.NetworkEntity.GetExtender<BonelabNetworkPlayer>();

        if (bonelabPlayer == null)
        {
            return;
        }

        bonelabPlayer.RigVitals.SetVitals(bodyVitals);
    }
}