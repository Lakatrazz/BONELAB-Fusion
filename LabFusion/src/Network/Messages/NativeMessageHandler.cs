using System.Reflection;

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
            tag = buffer[0];

            var messageSlice = buffer[1..];
            byte[] message = messageSlice.ToArray();

            if (Handlers[tag] != null)
            {
                Handlers[tag].Internal_HandleMessage(message, isServerHandled);
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

    public static readonly NativeMessageHandler[] Handlers = new NativeMessageHandler[byte.MaxValue];
}