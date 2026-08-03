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

        ClientSmallID? sender = route.Type == RelayType.None ? null : PlayerIDManager.LocalSmallID;

        using var message = NetMessage.Create(tag, writer, route, sender);

        Relay(message, route, sender);
    }

    public static void RelayModule<TMessage, TData>(TData data, MessageRoute route) where TMessage : ModuleMessageHandler where TData : INetSerializable
    {
        using var writer = NetWriter.Create(data.GetSize());

        data.Serialize(writer);

        ClientSmallID? sender = route.Type == RelayType.None ? null : PlayerIDManager.LocalSmallID;

        using var message = NetMessage.ModuleCreate<TMessage>(writer, route, sender);

        Relay(message, route, sender);
    }

    private static void Relay(NetMessage message, MessageRoute route, ClientSmallID? sender = null)
    {
        var type = route.Type;
        var channel = route.Channel;

        switch (type)
        {
            case RelayType.None:
            case RelayType.ToServer:
                ClientManager.SendToServer(message, channel);
                break;
            case RelayType.ToClients:
                if (ServerManager.IsServerRunning)
                {
                    ServerManager.SendToClients(message, channel);
                }
                else
                {
                    ClientManager.SendToServer(message, channel);
                }
                break;
            case RelayType.ToOtherClients:
                if (ServerManager.IsServerRunning)
                {
                    ServerManager.SendToClientsExcept(message, channel, PlayerIDManager.GetPlayerID(route.Target.Value).PlatformID);
                }
                else
                {
                    ClientManager.SendToServer(message, channel);
                }
                break;
            case RelayType.ToTarget:
                if (ServerManager.IsServerRunning)
                {
                    ServerManager.SendToClient(message, channel, PlayerIDManager.GetPlayerID(route.Target.Value).PlatformID);
                }
                else
                {
                    ClientManager.SendToServer(message, channel);
                }
                break;
            case RelayType.ToTargets:
                if (ServerManager.IsServerRunning)
                {
                    foreach (var target in route.Targets)
                    {
                        ServerManager.SendToClient(message, channel, PlayerIDManager.GetPlayerID(target).PlatformID);
                    }
                }
                else
                {
                    ClientManager.SendToServer(message, channel);
                }
                break;
        }
    }
}
