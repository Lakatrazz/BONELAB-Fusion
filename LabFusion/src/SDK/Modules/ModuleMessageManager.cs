using System.Buffers.Binary;
using System.Reflection;

using LabFusion.Extensions;
using LabFusion.Math;
using LabFusion.Network;
using LabFusion.Utilities;

namespace LabFusion.SDK.Modules;

/// <summary>
/// Manages the registration of <see cref="ModuleMessageHandler"/>s.
/// </summary>
public static class ModuleMessageManager
{
    /// <summary>
    /// Loads all <see cref="ModuleMessageHandler"/>s from an assembly.
    /// </summary>
    /// <param name="assembly">The assembly to load module messages from.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public static void LoadHandlers(Assembly assembly)
    {
        if (assembly == null)
        {
            throw new ArgumentNullException(nameof(assembly));
        }

        AssemblyUtilities.LoadAllValid<ModuleMessageHandler>(assembly, RegisterHandler);
    }

    /// <summary>
    /// Registers a <see cref="ModuleMessageHandler"/> from a type.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static void RegisterHandler<T>() where T : ModuleMessageHandler => RegisterHandler(typeof(T));

    /// <summary>
    /// Registers a <see cref="ModuleMessageHandler"/> from a type.
    /// </summary>
    /// <param name="type"></param>
    /// <exception cref="ArgumentException"></exception>
    public static void RegisterHandler(Type type)
    {
        if (TypeToHandlerLookup.ContainsKey(type))
        {
            throw new ArgumentException($"Handler {type.Name} was already registered.");
        }

        long tag = BitMath.MakeLong(type.Assembly.FullName.GetDeterministicHashCode(), type.AssemblyQualifiedName.GetDeterministicHashCode());

        if (TagToHandlerLookup.TryGetValue(tag, out var conflictingHandler))
        {
            throw new ArgumentException($"Handler {type.Name}'s hashed tag of {tag} conflicts with handler {conflictingHandler.GetType().Name}!");
        }

        var handler = CreateHandler(type, tag);
        Handlers.Add(handler);
        TypeToHandlerLookup[type] = handler;
        TagToHandlerLookup[tag] = handler;
    }

    private static ModuleMessageHandler CreateHandler(Type type, long tag)
    {
        var handler = (ModuleMessageHandler)Activator.CreateInstance(type);
        handler._tag = tag;
        handler.NetAttributes = type.GetCustomAttributes<Net.NetAttribute>().ToArray();
        return handler;
    }

    public static long? GetHandlerTagByType(Type type)
    {
        return GetHandlerByType(type)?.Tag;
    }

    public static ModuleMessageHandler GetHandlerByType(Type type)
    {
        if (TypeToHandlerLookup.TryGetValue(type, out var handler))
        {
            return handler;
        }

        return null;
    }

    public static bool PreRelayMessage(ReceivedMessage received)
    {
        try
        {
            var bytes = received.Bytes;

            long tag = GetTag(bytes);

            if (TagToHandlerLookup.TryGetValue(tag, out var handler))
            {
                var buffer = GetBuffer(bytes);

                var payload = new ReceivedMessage()
                {
                    Route = received.Route,
                    Bytes = buffer,
                    IsServerHandled = received.IsServerHandled,
                };

                return handler.ProcessPreRelayMessage(payload);
            }
        }
        catch (Exception e)
        {
            FusionLogger.LogException($"checking PreRelayMessage", e);

            return false;
        }

        return true;
    }

    public static void ReadMessage(ReceivedMessage received)
    {
        try
        {
            var bytes = received.Bytes;

            long tag = GetTag(bytes);

            if (TagToHandlerLookup.TryGetValue(tag, out var handler))
            {
                var buffer = GetBuffer(bytes);

                var payload = new ReceivedMessage()
                {
                    Route = received.Route,
                    Sender = received.Sender,
                    Bytes = buffer,
                    IsServerHandled = received.IsServerHandled,
                };

                handler.StartHandlingMessage(payload);
            }
        }
        catch (Exception e)
        {
            FusionLogger.LogException($"handling ModuleMessageHandler", e);
        }
    }

    private static long GetTag(byte[] bytes)
    {
        return BinaryPrimitives.ReadInt64BigEndian(bytes);
    }

    private static byte[] GetBuffer(byte[] bytes)
    {
        var buffer = new byte[bytes.Length - sizeof(long)];

        for (var i = 0; i < buffer.Length; i++)
        {
            buffer[i] = bytes[i + sizeof(long)];
        }

        return buffer;
    }

    public static Dictionary<Type, ModuleMessageHandler> TypeToHandlerLookup { get; private set; } = new();
    public static Dictionary<long, ModuleMessageHandler> TagToHandlerLookup { get; private set; } = new();
    public static List<ModuleMessageHandler> Handlers { get; private set; } = new();
}