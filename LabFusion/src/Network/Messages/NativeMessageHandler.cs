using System.Reflection;

using LabFusion.Exceptions;
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

    public static unsafe void ReadMessage(ReadOnlySpan<byte> buffer, bool isServerHandled = false)
    {
        int size = buffer.Length;
        NetworkInfo.BytesDown += size;

        byte tag = 0;

        try
        {
            using var reader = FusionReader.Create(buffer.ToArray());

            var prefix = reader.ReadFusionSerializable<MessagePrefix>();
            var message = reader.ReadBytes();

            tag = prefix.Tag;

            if (Handlers[tag] != null)
            {
                var payload = new ReceivedMessage()
                {
                    Type = prefix.Type,
                    Channel = prefix.Channel,
                    Sender = prefix.Sender,
                    Target = prefix.Target,
                    Bytes = message,
                    IsServerHandled = isServerHandled,
                };

                Handlers[tag].Internal_HandleMessage(payload);
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

        switch (received.Type)
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
                    using var message = FusionMessage.Create(Tag, received);

                    MessageSender.BroadcastMessage(received.Channel, message);

                    return;
                }
                break;
            case RelayType.ToOtherClients:
                if (received.IsServerHandled)
                {
                    using var message = FusionMessage.Create(Tag, received);

                    MessageSender.BroadcastMessageExcept(received.Sender.Value, received.Channel, message, false);

                    return;
                }
                break;
            case RelayType.ToTarget:
                if (received.IsServerHandled)
                {
                    using var message = FusionMessage.Create(Tag, received);

                    MessageSender.SendFromServer(received.Target.Value, received.Channel, message);

                    return;
                }
                break;
        }

        OnHandleMessage(received);

        HandleMessage(received.Bytes, received.IsServerHandled);
    }

    public static readonly NativeMessageHandler[] Handlers = new NativeMessageHandler[byte.MaxValue];
}