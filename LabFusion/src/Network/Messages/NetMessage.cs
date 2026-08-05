using LabFusion.Network.Serialization;
using LabFusion.SDK.Modules;

using System.Buffers.Binary;
using System.Runtime.InteropServices;

namespace LabFusion.Network;

/// <summary>
/// A message that can be sent over the network.
/// <para>Allocates unmanaged memory and implements IDisposable, so make sure that you either wrap the message in a using block or manually dispose of it when done.</para>
/// </summary>
public unsafe class NetMessage : IDisposable
{
    /// <summary>
    /// The amount of bytes written to <see cref="Buffer"/>.
    /// </summary>
    public int Length { get; private set; } = 0;

    /// <summary>
    /// The pointer to the unmanaged byte array.
    /// </summary>
    public byte* Buffer { get; private set; } = null;

    /// <summary>
    /// Whether the message and its buffer have already been disposed.
    /// </summary>
    public bool IsDisposed { get; private set; } = false;

    /// <summary>
    /// Creates a sendable native message given serializable data that is automatically written.
    /// </summary>
    /// <typeparam name="TData"></typeparam>
    /// <param name="data"></param>
    /// <param name="tag"></param>
    /// <param name="route"></param>
    /// <param name="sender"></param>
    /// <returns></returns>
    public static NetMessage CreateNative<TData>(TData data, byte tag, MessageRoute route, ClientSmallID? sender = null) where TData : INetSerializable
    {
        using var writer = NetWriter.Create(data.GetSize());

        data.Serialize(writer);

        return CreateNative(tag, writer, route, sender);
    }

    /// <summary>
    /// Creates a sendable native message from its tag, the data from a writer, route, and sender.
    /// </summary>
    /// <param name="tag"></param>
    /// <param name="writer"></param>
    /// <param name="route"></param>
    /// <param name="sender"></param>
    /// <returns></returns>
    public static NetMessage CreateNative(byte tag, NetWriter writer, MessageRoute route, ClientSmallID? sender = null)
    {
        return CreateNative(tag, writer.Buffer, route, sender);
    }

    /// <summary>
    /// Creates a sendable native message from its tag, data buffer, route, and sender.
    /// </summary>
    /// <param name="tag"></param>
    /// <param name="buffer"></param>
    /// <param name="route"></param>
    /// <param name="sender"></param>
    /// <returns></returns>
    public static NetMessage CreateNative(byte tag, ArraySegment<byte> buffer, MessageRoute route, ClientSmallID? sender = null)
    {
        var prefix = new MessagePrefix()
        {
            Tag = tag,
            Route = route,
            SenderSmallID = sender,
        };

        using var writer = NetWriter.Create(prefix.GetSize().Value + buffer.Count + sizeof(int));

        writer.SerializeValue(ref prefix);
        writer.Write(buffer);

        int size = writer.Length;
        var message = CreateEmpty(size);

        for (var i = 0; i < size; i++)
        {
            message.Buffer[i] = writer.Buffer[i];
        }

        return message;
    }

    /// <summary>
    /// Recreates a sendable native message from a received message.
    /// </summary>
    /// <param name="tag"></param>
    /// <param name="received"></param>
    /// <returns></returns>
    public static NetMessage CreateNative(byte tag, ReceivedMessage received)
    {
        var prefix = new MessagePrefix()
        {
            Tag = tag,
            Route = received.Route,
            SenderSmallID = received.SenderSmallID,
        };

        using var writer = NetWriter.Create(prefix.GetSize().Value + received.Bytes.Length + sizeof(int));

        writer.SerializeValue(ref prefix);
        writer.Write(received.Bytes);

        int size = writer.Length;
        var message = CreateEmpty(size);

        for (var i = 0; i < size; i++)
        {
            message.Buffer[i] = writer.Buffer[i];
        }

        return message;
    }

    /// <summary>
    /// Creates a sendable module message given serializable data that is automatically written.
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    /// <typeparam name="TData"></typeparam>
    /// <param name="data"></param>
    /// <param name="route"></param>
    /// <param name="sender"></param>
    /// <returns></returns>
    public static NetMessage CreateModule<TMessage, TData>(TData data, MessageRoute route, ClientSmallID? sender = null) where TMessage : ModuleMessageHandler where TData : INetSerializable
    {
        using var writer = NetWriter.Create(data.GetSize());

        data.Serialize(writer);

        return CreateModule<TMessage>(writer, route, sender);
    }

    /// <summary>
    /// Creates a sendable module message from its type, the data from a writer, route, and sender.
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    /// <param name="writer"></param>
    /// <param name="route"></param>
    /// <param name="sender"></param>
    /// <returns></returns>
    public static NetMessage CreateModule<TMessage>(NetWriter writer, MessageRoute route, ClientSmallID? sender = null) where TMessage : ModuleMessageHandler
    {
        return CreateModule(typeof(TMessage), writer, route, sender);
    }

    /// <summary>
    /// Creates a sendable module message from its type, the data from a writer, route, and sender.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="writer"></param>
    /// <param name="route"></param>
    /// <param name="sender"></param>
    /// <returns></returns>
    public static NetMessage CreateModule(Type type, NetWriter writer, MessageRoute route, ClientSmallID? sender = null)
    {
        return CreateModule(type, writer.Buffer, route, sender);
    }

    /// <summary>
    /// Creates a sendable module message from its type, data buffer, route, and sender.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="buffer"></param>
    /// <param name="route"></param>
    /// <param name="sender"></param>
    /// <returns></returns>
    public static NetMessage CreateModule(Type type, ArraySegment<byte> buffer, MessageRoute route, ClientSmallID? sender = null)
    {
        // Assign the module type
        var tag = ModuleMessageManager.GetHandlerTagByType(type);

        if (!tag.HasValue)
        {
            return null;
        }

        var value = tag.Value;

        var prefix = new MessagePrefix()
        {
            Tag = NativeMessageTag.Module,
            Route = route,
            SenderSmallID = sender,
        };

        using var writer = NetWriter.Create(prefix.GetSize().Value + buffer.Count + sizeof(long) + sizeof(int));

        writer.SerializeValue(ref prefix);

        var expandedBuffer = new byte[buffer.Count + sizeof(long)];

        BinaryPrimitives.WriteInt64BigEndian(expandedBuffer, value);

        for (var i = 0; i < buffer.Count; i++)
        {
            expandedBuffer[i + sizeof(long)] = buffer[i];
        }

        writer.Write(expandedBuffer);

        int size = writer.Length;
        var message = CreateEmpty(size);

        for (var i = 0; i < size; i++)
        {
            message.Buffer[i] = writer.Buffer[i];
        }

        return message;
    }

    /// <summary>
    /// Converts the message into a managed byte array.
    /// </summary>
    /// <returns></returns>
    public byte[] ToByteArray()
    {
        var bytes = new byte[Length];

        Marshal.Copy((IntPtr)Buffer, bytes, 0, Length);

        return bytes;
    }

    /// <summary>
    /// Frees the unmanaged memory allocated for the message.
    /// </summary>
    public void Dispose()
    {
        if (IsDisposed)
        {
            return;
        }

        GC.SuppressFinalize(this);
        Marshal.FreeHGlobal((IntPtr)Buffer);

        IsDisposed = true;
    }

    /// <summary>
    /// Creates an empty message from the amount of data that needs to be written.
    /// </summary>
    /// <param name="size"></param>
    /// <returns></returns>
    private static NetMessage CreateEmpty(int size)
    {
        return new NetMessage()
        {
            Length = size,
            Buffer = (byte*)Marshal.AllocHGlobal(size),
        };
    }
}