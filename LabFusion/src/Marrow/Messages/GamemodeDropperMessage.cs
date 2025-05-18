using LabFusion.SDK.Modules;
using LabFusion.Network;
using LabFusion.Network.Serialization;
using LabFusion.Entities;
using LabFusion.SDK.MonoBehaviours;

namespace LabFusion.Marrow;

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

        data.Entity.HookEntityRegistered((entity) =>
        {
            var propExtender = entity.GetExtender<NetworkProp>();
            var pooleeExtender = entity.GetExtender<PooleeExtender>();

            if (propExtender != null && pooleeExtender != null)
            {
                var droppedItem = propExtender.MarrowEntity.gameObject.AddComponent<DroppedItem>();

                droppedItem.Initialize(entity, pooleeExtender.Component);
            }
        });
    }
}
