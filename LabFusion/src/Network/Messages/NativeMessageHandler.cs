using System.Reflection;

using LabFusion.Exceptions;
using LabFusion.Network.Serialization;
using LabFusion.Utilities;

namespace LabFusion.Network;

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
        int size = message.Buffer.Length;
        NetworkInfo.BytesDown += size;

        byte tag = 0;

        try
        {
            using var reader = NetReader.Create(message.Buffer.ToArray());

            MessagePrefix prefix = null;
            reader.SerializeValue(ref prefix);

            var bytes = reader.ReadBytes();

            tag = prefix.Tag;

            if (Handlers[tag] != null)
            {
                var payload = new ReceivedMessage()
                {
                    Route = prefix.Route,
                    Sender = prefix.Sender,
                    Bytes = bytes,
                    IsServerHandled = message.IsServerHandled,
                };

                Handlers[tag].StartHandlingMessage(payload);
            }
#if DEBUG
            else
            {
                FusionLogger.Warn($"Received message with invalid tag {tag}!");
            }
#endif
        }
        catch (Exception e)
        {
            FusionLogger.Error($"Failed handling network message of tag {tag} with reason: {e.Message}\nTrace:{e.StackTrace}");
        }
    }

    public sealed override void Handle(ReceivedMessage received)
    {
        if (ExpectedReceiver == ExpectedReceiverType.ServerOnly && !received.IsServerHandled)
        {
            throw new ExpectedServerException();
        }
        else if (ExpectedReceiver == ExpectedReceiverType.ClientsOnly && received.IsServerHandled)
        {
            throw new ExpectedClientException();
        }

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
                    throw new ExpectedServerException();
                }
                break;
            case RelayType.ToClients:
                if (received.IsServerHandled)
                {
                    using var message = NetMessage.Create(Tag, received);

                    MessageSender.BroadcastMessage(channel, message);

                    return;
                }
                break;
            case RelayType.ToOtherClients:
                if (received.IsServerHandled)
                {
                    using var message = NetMessage.Create(Tag, received);

                    MessageSender.BroadcastMessageExcept(received.Sender.Value, channel, message, false);

                    return;
                }
                break;
            case RelayType.ToTarget:
                if (received.IsServerHandled)
                {
                    using var message = NetMessage.Create(Tag, received);

                    MessageSender.SendFromServer(route.Target.Value, channel, message);

                    return;
                }
                break;
            case RelayType.ToTargets:
                if (received.IsServerHandled)
                {
                    using var message = NetMessage.Create(Tag, received);

                    foreach (var target in route.Targets)
                    {
                        MessageSender.SendFromServer(target, channel, message);
                    }

                    return;
                }
                break;
        }

        OnHandleMessage(received);
    }

    public static readonly NativeMessageHandler[] Handlers = new NativeMessageHandler[byte.MaxValue];
}