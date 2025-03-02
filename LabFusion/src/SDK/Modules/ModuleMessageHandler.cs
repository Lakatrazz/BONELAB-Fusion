using System.Reflection;

using LabFusion.Exceptions;
using LabFusion.Network;
using LabFusion.Utilities;
using static Il2CppSystem.Uri;

namespace LabFusion.SDK.Modules;

public abstract class ModuleMessageHandler : MessageHandler
{
    internal ushort? _tag = null;
    public ushort? Tag => _tag;

    public static void LoadHandlers(Assembly assembly)
    {
        if (assembly == null)
        {
            throw new NullReferenceException("Tried loading handlers from a null module assembly!");
        }

        AssemblyUtilities.LoadAllValid<ModuleMessageHandler>(assembly, RegisterHandler);
    }

    public static void RegisterHandler<T>() where T : ModuleMessageHandler => RegisterHandler(typeof(T));

    protected static void RegisterHandler(Type type)
    {
        if (HandlerTypes.ContainsKey(type.AssemblyQualifiedName))
        {
            throw new ArgumentException($"Handler {type.Name} was already registered.");
        }

        HandlerTypes.Add(type.AssemblyQualifiedName, type);
    }

    public static string[] GetExistingTypeNames()
    {
        return HandlerTypes.Keys.ToArray();
    }

    public static void PopulateHandlerTable(string[] names)
    {
        Handlers = new ModuleMessageHandler[names.Length];

        for (ushort i = 0; i < names.Length; i++)
        {
            string handlerName = names[i];

            if (HandlerTypes.ContainsKey(handlerName))
            {
                Type type = HandlerTypes[handlerName];
                var handler = CreateHandler(type, i);

                Handlers[i] = handler;
                HandlerLookup[type] = handler;
            }
            else
            {
                Handlers[i] = null;
            }
        }
    }

    public static void ClearHandlerTable()
    {
        Handlers = null;
    }

    private static ModuleMessageHandler CreateHandler(Type type, ushort tag)
    {
        var handler = (ModuleMessageHandler)Activator.CreateInstance(type);
        handler._tag = tag;
        handler.NetAttributes = type.GetCustomAttributes<Net.NetAttribute>().ToArray();
        return handler;
    }

    public static ushort? GetHandlerTag(Type type)
    {
        if (Handlers == null)
        {
            return null;
        }

        if (HandlerLookup.TryGetValue(type, out var handler))
        {
            return handler.Tag;
        }

        return null;
    }

    public static void ReadMessage(ReceivedMessage received)
    {
        if (Handlers == null)
        {
#if DEBUG
            FusionLogger.Warn("We received a ModuleMessage, but Handlers were not registered yet.");
#endif

            return;
        }

        try
        {
            var bytes = received.Bytes;

            ushort tag = (ushort)((bytes[0] << 8) | bytes[1]);
            var buffer = new byte[bytes.Length - sizeof(ushort)];

            for (var i = 0; i < buffer.Length; i++)
            {
                buffer[i] = received.Bytes[i + 2];
            }

            // Since modules cannot be assumed to exist for everyone, we need to null check
            if (Handlers.Length > tag && Handlers[tag] != null)
            {
                var payload = new ReceivedMessage()
                {
                    Type = received.Type,
                    Channel = received.Channel,
                    Sender = received.Sender,
                    Target = received.Target,
                    Bytes = buffer,
                    IsServerHandled = received.IsServerHandled,
                };

                Handlers[tag].Internal_HandleMessage(payload);
            }
        }
        catch (Exception e)
        {
            FusionLogger.Error($"Failed handling network message with reason: {e.Message}\nTrace:{e.StackTrace}");
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

        OnHandleMessage(received);
    }

    public static Dictionary<string, Type> HandlerTypes { get; private set; } = new();
    public static Dictionary<Type, ModuleMessageHandler> HandlerLookup { get; private set; } = new();
    public static ModuleMessageHandler[] Handlers { get; private set; } = null;
}