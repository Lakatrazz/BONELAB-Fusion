using System.Reflection;

using LabFusion.Exceptions;
using LabFusion.Network.Serialization;
using LabFusion.Player;
using LabFusion.Utilities;

namespace LabFusion.Network;

/// <summary>
/// A message handler for a message built into Fusion.
/// This should never be extended outside of Fusion. 
/// <para>For registering additional messages in Modules, the <see cref="LabFusion.SDK.Modules.ModuleMessageHandler"/> should be used instead.</para>
/// </summary>
public abstract class NativeMessageHandler : MessageHandler
{
    public abstract byte Tag { get; }

    // Handlers are created up front, they're not static
    public static void RegisterHandlersFromAssembly(Assembly targetAssembly)
    {
        if (targetAssembly == null) throw new NullReferenceException("Can't register from a null assembly!");

#if DEBUG
        FusionLogger.Log($"Populating MessageHandler list from {targetAssembly.GetName().Name}!");
#endif

        AssemblyUtilities.LoadAllValid<NativeMessageHandler>(targetAssembly, RegisterHandler);
    }

    public static void RegisterHandler<T>() where T : NativeMessageHandler => RegisterHandler(typeof(T));

    protected static void RegisterHandler(Type type)
    {
        NativeMessageHandler handler = Activator.CreateInstance(type) as NativeMessageHandler;

        handler.NetAttributes = type.GetCustomAttributes<Net.NetAttribute>().ToArray();

        byte index = handler.Tag;

        if (Handlers[index] != null) 
        { 
            throw new Exception($"{type.Name} has the same index as {Handlers[index].GetType().Name}, we can't replace handlers!"); 
        }

#if DEBUG
        FusionLogger.Log($"Registered {type.Name}");
#endif

        Handlers[index] = handler;
    }

    public static unsafe void ReadMessage(ReadableMessage message)
    {
        bool isServerHandled = message.IsServerHandled;

        int size = message.Buffer.Length;
        NetworkInfo.BytesDown += size;

        byte tag = 0;

        try
        {
            using var reader = NetReader.Create(message.Buffer.ToArray());

            MessagePrefix prefix = null;
            reader.SerializeValue(ref prefix);

            MessageRoute route = prefix.Route;

            var bytes = reader.ReadBytes();

            tag = prefix.Tag;

            var handler = Handlers[tag];

            if (handler == null)
            {
#if DEBUG
                FusionLogger.Warn($"Received native message with invalid tag {tag}!");
#endif
                return;
            }

            if (!handler.ValidateMessageSender(message, prefix, out var senderPlatformID, out var senderSmallID))
            {
                return;
            }

            var payload = new ReceivedMessage()
            {
                Route = route,
                SenderPlatformID = senderPlatformID,
                SenderSmallID = senderSmallID,
                Bytes = bytes,
                IsServerHandled = message.IsServerHandled,
            };

            handler.StartHandlingMessage(payload);
        }
        catch (Exception e)
        {
            FusionLogger.LogException($"handling native message of tag {tag}", e);
        }
    }

    private bool ValidateMessageSender(ReadableMessage message, MessagePrefix prefix, out ClientPlatformID? senderPlatformID, out ClientSmallID? senderSmallID)
    {
        bool isServerHandled = message.IsServerHandled;

        if (isServerHandled)
        {
            return ServerValidateMessageSender(message, out senderPlatformID, out senderSmallID);
        }

        return ClientValidateMessageSender(prefix, out senderPlatformID, out senderSmallID);
    }

    private bool ServerValidateMessageSender(ReadableMessage message, out ClientPlatformID? senderPlatformID, out ClientSmallID? senderSmallID)
    {
        senderPlatformID = message.SenderPlatformID;
        senderSmallID = null;

        if (!senderPlatformID.HasValue)
        {
            FusionLogger.Warn("Server received a ReadableMessage with no SenderPlatformID set! Make sure the NetworkLayer is properly setting the sender when the server receives a message!");
            return false;
        }

        if (PlayerIDManager.TryGetSmallID(senderPlatformID.Value, out var existingSmallID))
        {
            senderSmallID = existingSmallID;
        }

        if (!senderSmallID.HasValue && !AllowConnectingClients)
        {
            FusionLogger.Warn($"Server received an unauthorized message of tag {Tag} from client with PlatformID {senderPlatformID.Value} while they were still connecting! Disconnecting client!");
            
            NetworkConnectionManager.DisconnectUser(senderPlatformID.Value);

            return false;
        }

        return true;
    }

    private static bool ClientValidateMessageSender(MessagePrefix prefix, out ClientPlatformID? senderPlatformID, out ClientSmallID? senderSmallID)
    {
        senderPlatformID = null;
        senderSmallID = prefix.SenderSmallID;

        if (senderSmallID.HasValue && PlayerIDManager.TryGetPlatformID(senderSmallID.Value, out var existingPlatformID))
        {
            senderPlatformID = existingPlatformID;
        }

        return true;
    }

    public sealed override void Handle(ReceivedMessage received)
    {
        CheckExpectedReceiver(received);

        if (received.IsServerHandled && !OnPreRelayMessage(received))
        {
            return;
        }

        var route = received.Route;
        var type = route.Type;
        var channel = route.Channel;

        switch (type)
        {
            case RelayType.ToServer:
                if (!received.IsServerHandled)
                {
                    throw new MessageExpectedServerException();
                }
                break;
            case RelayType.ToClients:
                if (received.IsServerHandled)
                {
                    using var message = NetMessage.Create(Tag, received);

                    ServerManager.SendToClients(message, channel);
                    return;
                }
                break;
            case RelayType.ToOtherClients:
                if (received.IsServerHandled)
                {
                    using var message = NetMessage.Create(Tag, received);

                    ServerManager.SendToClientsExcept(message, channel, PlayerIDManager.GetPlayerID(received.SenderSmallID.Value).PlatformID);
                    return;
                }
                break;
            case RelayType.ToTarget:
                if (received.IsServerHandled)
                {
                    using var message = NetMessage.Create(Tag, received);

                    ServerManager.SendToClient(message, channel, PlayerIDManager.GetPlayerID(route.Target.Value).PlatformID);
                    return;
                }
                break;
            case RelayType.ToTargets:
                if (received.IsServerHandled)
                {
                    using var message = NetMessage.Create(Tag, received);

                    foreach (var target in route.Targets)
                    {
                        ServerManager.SendToClient(message, channel, PlayerIDManager.GetPlayerID(target).PlatformID);
                    }
                    return;
                }
                break;
        }

        OnHandleMessage(received);
    }

    public static readonly NativeMessageHandler[] Handlers = new NativeMessageHandler[byte.MaxValue];
}