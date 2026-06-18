using LabFusion.Entities;
using LabFusion.Marrow.Data;
using LabFusion.Network;
using LabFusion.Network.Serialization;

using LabFusion.SDK.Modules;

namespace LabFusion.Marrow.Messages;

public class EntityRepresentationResponseData : INetSerializable
{
    public NetworkEntityReference Entity;

    public MarrowEntityRepresentation Representation;

    public int? GetSize() => Entity.GetSize() + Representation.GetSize();

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref Entity);
        serializer.SerializeValue(ref Representation);
    }
}

public class EntityRepresentationResponseMessage : ModuleMessageHandler
{
    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<EntityRepresentationResponseData>();

        if (!data.Entity.TryGetEntity(out var entity))
        {
            return;
        }

        var representationExtender = entity.GetExtender<IMarrowEntityRepresentationExtender>();

        if (representationExtender == null)
        {
            return;
        }

        representationExtender.OnRepresentationReceived(data.Representation);
    }
}
