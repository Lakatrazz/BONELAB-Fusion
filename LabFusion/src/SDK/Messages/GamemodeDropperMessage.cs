using LabFusion.SDK.Modules;
using LabFusion.Network;
using LabFusion.Network.Serialization;
using LabFusion.Entities;
using LabFusion.SDK.MonoBehaviours;

namespace LabFusion.SDK.Messages;

public class GamemodeDropperData : INetSerializable
{
    public NetworkEntityReference Entity;

    public int? GetSize() => Entity.GetSize();

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref Entity);
    }
}

[Net.DelayWhileTargetLoading]
public class GamemodeDropperMessage : ModuleMessageHandler
{
    public override ExpectedReceiverType ExpectedReceiver => ExpectedReceiverType.ClientsOnly;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<GamemodeDropperData>();

        if (!data.Entity.TryGetEntity(out var entity))
        {
            return;
        }

        var propExtender = entity.GetExtender<NetworkProp>();
        var pooleeExtender = entity.GetExtender<PooleeExtender>();

        if (propExtender != null && pooleeExtender != null)
        {
            var gamemodeItem = propExtender.MarrowEntity.gameObject.AddComponent<GamemodeItem>();

            gamemodeItem.Initialize(entity, pooleeExtender.Component);
        }
    }
}
