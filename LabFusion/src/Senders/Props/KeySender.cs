using LabFusion.Network;
using LabFusion.Player;

using UnityEngine;

namespace LabFusion.Senders;

public static class KeySender
{
    public static void SendStaticKeySlot(ushort keyId, GameObject receiver)
    {
        var data = KeySlotData.Create(PlayerIdManager.LocalSmallId, KeySlotType.INSERT_STATIC, keyId, receiver);

        MessageRelay.RelayNative(data, NativeMessageTag.KeySlot, NetworkChannel.Reliable, RelayType.ToOtherClients);
    }

    public static void SendPropKeySlot(ushort keyId, ushort receiverId, byte receiverIndex)
    {
        var data = KeySlotData.Create(PlayerIdManager.LocalSmallId, KeySlotType.INSERT_PROP, keyId, null, receiverId, receiverIndex);

        MessageRelay.RelayNative(data, NativeMessageTag.KeySlot, NetworkChannel.Reliable, RelayType.ToOtherClients);
    }
}
