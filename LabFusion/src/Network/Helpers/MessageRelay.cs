using LabFusion.Network.Serialization;
using LabFusion.Player;
using LabFusion.SDK.Modules;

namespace LabFusion.Network;

public static class MessageRelay
{
    public static void RelayNative<TData>(TData data, byte tag, MessageRoute route) where TData : INetSerializable
    {
        using var writer = NetWriter.Create(data.GetSize());

        data.Serialize(writer);

        byte? sender = route.Type == RelayType.None ? null : PlayerIDManager.LocalSmallID;

        using var message = NetMessage.Create(tag, writer, route, sender);

        Relay(message, route, sender);
    }

    public static void RelayModule<TMessage, TData>(TData data, MessageRoute route) where TMessage : ModuleMessageHandler where TData : INetSerializable
    {
        using var writer = NetWriter.Create(data.GetSize());

        data.Serialize(writer);

        byte? sender = route.Type == RelayType.None ? null : PlayerIDManager.LocalSmallID;

        using var message = NetMessage.ModuleCreate<TMessage>(writer, route, sender);

        Relay(message, route, sender);
    }

    private static void Relay(NetMessage message, MessageRoute route, byte? sender = null)
    {
        var type = route.Type;
        var channel = route.Channel;

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
                    MessageSender.SendFromServer(route.Target.Value, channel, message);
                }
                else
                {
                    MessageSender.SendToServer(channel, message);
                }
                break;
            case RelayType.ToTargets:
                if (NetworkInfo.IsHost)
                {
                    foreach (var target in route.Targets)
                    {
                        MessageSender.SendFromServer(target, channel, message);
                    }
                }
                else
                {
                    MessageSender.SendToServer(channel, message);
                }
                break;
        }
    }
}
