using System.Reflection;

using LabFusion.Utilities;
using LabFusion.Data;
using LabFusion.Network;
using LabFusion.Network.Serialization;

namespace LabFusion.Grabbables;

public enum GrabGroup : byte
{
    UNKNOWN,
    ENTITY,
    STATIC,
    WORLD,
}

public abstract class GrabGroupHandler<T> : GrabGroupHandler where T : SerializedGrab, new()
{
    public override void HandleGrab(ref SerializedGrab serializedGrab, INetSerializer serializer)
    {
        var genericGrab = serializedGrab as T;

        serializer.SerializeValue(ref genericGrab);

        serializedGrab = genericGrab;
    }
}

public abstract class GrabGroupHandler
{
    public virtual GrabGroup? Group { get; } = null;

    public abstract void HandleGrab(ref SerializedGrab serializedGrab, INetSerializer serializer);

    // Handlers are created up front, they're not static
    public static void RegisterHandlersFromAssembly(Assembly targetAssembly)
    {
        if (targetAssembly == null) throw new NullReferenceException("Can't register from a null assembly!");

#if DEBUG
        FusionLogger.Log($"Populating GrabHandler list from {targetAssembly.GetName().Name}!");
#endif

        AssemblyUtilities.LoadAllValid<GrabGroupHandler>(targetAssembly, RegisterHandler);
    }

    public static void RegisterHandler<T>() where T : NativeMessageHandler => RegisterHandler(typeof(T));

    protected static void RegisterHandler(Type type)
    {
        GrabGroupHandler handler = Activator.CreateInstance(type) as GrabGroupHandler;

        if (handler.Group == null)
        {
            FusionLogger.Warn($"Didn't register {type.Name} because its grab group was null!");
        }
        else
        {
            byte index = (byte)handler.Group.Value;

            if (Handlers[index] != null) throw new Exception($"{type.Name} has the same index as {Handlers[index].GetType().Name}, we can't replace grab handlers!");

#if DEBUG
            FusionLogger.Log($"Registered {type.Name}");
#endif

            Handlers[index] = handler;
        }
    }

    public static void SerializeGrab(ref SerializedGrab grab, INetSerializer serializer, GrabGroup group)
    {
        try
        {
            Handlers[(byte)group].HandleGrab(ref grab, serializer);
        }
        catch (Exception e)
        {
            FusionLogger.Error($"Failed handling serialized grab with reason: {e.Message}\nTrace:{e.StackTrace}");
        }
    }


    public static readonly GrabGroupHandler[] Handlers = new GrabGroupHandler[byte.MaxValue];
}