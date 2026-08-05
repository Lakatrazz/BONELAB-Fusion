using LabFusion.Entities;

namespace LabFusion.Network.Messages;

public static class CommonMessageValidation
{
    public static bool ValidateSenderOwnsEntity(ReceivedMessage received)
    {
        var entityReference = received.ReadData<NetworkEntityReference>();

        var sender = received.SenderSmallID.Value;

        return ValidateSenderOwnsEntity(entityReference, sender);
    }

    public static bool ValidateSenderOwnsEntity(NetworkEntityReference entityReference, ClientSmallID sender)
    {
        if (entityReference.TryGetEntity(out var networkEntity) && networkEntity.HasOwner && networkEntity.OwnerID.SmallID != sender)
        {
            return false;
        }

        return true;
    }
}
