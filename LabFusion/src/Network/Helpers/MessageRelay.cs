using LabFusion.Network.Serialization;
using LabFusion.Player;
using LabFusion.SDK.Modules;

namespace LabFusion.Network;

public static class MessageRelay
{
    public static void RelayNative<TData>(TData data, byte tag, NetworkChannel channel, RelayType type, byte? target = null) where TData : INetSerializable
    {
        using var writer = NetWriter.Create(data.GetSize());

        data.Serialize(writer);

        byte? sender = type == RelayType.None ? null : PlayerIDManager.LocalSmallID;

        using var message = FusionMessage.Create(tag, writer, type, channel, sender, target);

        Relay(message, channel, type, sender, target);
    }

    public static void RelayModule<TMessage, TData>(TData data, NetworkChannel channel, RelayType type, byte? target = null) where TMessage : ModuleMessageHandler where TData : INetSerializable
    {
        using var writer = NetWriter.Create(data.GetSize());

        data.Serialize(writer);

        byte? sender = type == RelayType.None ? null : PlayerIDManager.LocalSmallID;

        using var message = FusionMessage.ModuleCreate<TMessage>(writer, type, channel, sender, target);

        Relay(message, channel, type, sender, target);
    }

    private static void Relay(FusionMessage message, NetworkChannel channel, RelayType type, byte? sender = null, byte? target = null)
    {
        switch (type)
        {
            case RelayType.None:
            case RelayType.ToServer:
                MessageSender.SendToServer(channel, message);
                break;
            case RelayType.ToClients:
                if (NetworkInfo.IsHost)
                {
                    MessageSender.BroadcastMessage(channel, message);
                }
                else
                {
                    MessageSender.SendToServer(channel, message);
                }
                break;
            case RelayType.ToOtherClients:
                if (NetworkInfo.IsHost)
                {
                    MessageSender.BroadcastMessageExcept(sender.Value, channel, message, false);
                }
                else
                {
                    MessageSender.SendToServer(channel, message);
                }
                break;
            case RelayType.ToTarget:
                if (NetworkInfo.IsHost)
                {
                    MessageSender.SendFromServer(target.Value, channel, message);
                }
                else
                {
                    MessageSender.SendToServer(channel, message);
                }
                break;
        }
    }
}
