using LabFusion.Entities;
using LabFusion.Marrow.Data;
using LabFusion.Network;
using LabFusion.SDK.Modules;

namespace LabFusion.Marrow.Messages;

public class EntityRepresentationRequestMessage : ModuleMessageHandler
{
    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var sender = received.Sender;

        if (!sender.HasValue)
        {
            return;
        }

        var data = received.ReadData<NetworkEntityReference>();

        if (!data.TryGetEntity(out var entity))
        {
            return;
        }

        var marrowEntityExtender = entity.GetExtender<IMarrowEntityExtender>();

        if (marrowEntityExtender == null || marrowEntityExtender.MarrowEntity == null)
        {
            return;
        }

        var marrowEntity = marrowEntityExtender.MarrowEntity;

        var representation = MarrowEntityRepresentation.CreateFromEntity(marrowEntity);

        var responseData = new EntityRepresentationResponseData()
        {
            Entity = data,
            Representation = representation,
        };

        MessageRelay.RelayModule<EntityRepresentationResponseMessage, EntityRepresentationResponseData>(responseData, new MessageRoute(sender.Value, NetworkChannel.Reliable));
    }
}
