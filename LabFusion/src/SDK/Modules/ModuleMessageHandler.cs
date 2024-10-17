using System.Reflection;

using LabFusion.Data;
using LabFusion.Network;
using LabFusion.Utilities;

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

        for (ushort i = 0; i < Handlers.Length; i++)
        {
            var other = Handlers[i];

            // We don't have one for this slot. Skip.
            if (other == null)
            {
                continue;
            }

            if (other.GetType().AssemblyQualifiedName == type.AssemblyQualifiedName)
            {
                return i;
            }
        }

        return null;
    }

    public static void ReadMessage(byte[] bytes, bool isServerHandled = false)
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
            ushort tag = BitConverter.ToUInt16(bytes, 0);
            byte[] buffer = ByteRetriever.Rent(bytes.Length - 2);

            for (var i = 0; i < buffer.Length; i++)
            {
                buffer[i] = bytes[i + 2];
            }

            // Since modules cannot be assumed to exist for everyone, we need to null check
            if (Handlers.Length > tag && Handlers[tag] != null)
            {
                Handlers[tag].Internal_HandleMessage(buffer, isServerHandled);
            }
        }
        catch (Exception e)
        {
            FusionLogger.Error($"Failed handling network message with reason: {e.Message}\nTrace:{e.StackTrace}");
        }
    }

    public static FusionDictionary<string, Type> HandlerTypes { get; private set; } = new();
    public static ModuleMessageHandler[] Handlers { get; private set; } = null;
}