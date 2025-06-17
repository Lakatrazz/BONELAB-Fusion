using LabFusion.Network.Serialization;
using LabFusion.SDK.Modules;

using System.Buffers.Binary;
using System.Runtime.InteropServices;

namespace LabFusion.Network;

public unsafe class NetMessage : IDisposable
{
    private byte* _buffer;
    private int _size;

    private bool _disposed;

    public int Length
    {
        get
        {
            return _size;
        }
    }

    public byte* Buffer
    {
        get
        {
            return _buffer;
        }
    }

    private static NetMessage Create(int size)
    {
        return new NetMessage()
        {
            _buffer = (byte*)Marshal.AllocHGlobal(size),
            _size = size,
            _disposed = false,
        };
    }

    public static NetMessage Create(byte tag, NetWriter writer, MessageRoute route, byte? sender = null)
    {
        return Create(tag, writer.Buffer, route, sender);
    }

    public static NetMessage Create(byte tag, ArraySegment<byte> buffer, MessageRoute route, byte? sender = null)
    {
        var prefix = new MessagePrefix()
        {
            Tag = tag,
            Route = route,
            Sender = sender,
        };

        using var writer = NetWriter.Create(prefix.GetSize().Value + buffer.Count + sizeof(int));

        writer.SerializeValue(ref prefix);
        writer.Write(buffer);

        int size = writer.Length;
        var message = Create(size);

        for (var i = 0; i < size; i++)
        {
            message._buffer[i] = writer.Buffer[i];
        }

        return message;
    }

    public static NetMessage Create(byte tag, ReceivedMessage received)
    {
        var prefix = new MessagePrefix()
        {
            Tag = tag,
            Route = received.Route,
            Sender = received.Sender,
        };

        using var writer = NetWriter.Create(prefix.GetSize().Value + received.Bytes.Length + sizeof(int));

        writer.SerializeValue(ref prefix);
        writer.Write(received.Bytes);

        int size = writer.Length;
        var message = Create(size);

        for (var i = 0; i < size; i++)
        {
            message._buffer[i] = writer.Buffer[i];
        }

        return message;
    }

    public static NetMessage ModuleCreate<TMessage>(NetWriter writer, MessageRoute route, byte? sender = null) where TMessage : ModuleMessageHandler
    {
        return ModuleCreate(typeof(TMessage), writer, route, sender);
    }

    public static NetMessage ModuleCreate<TMessage>(byte[] buffer, MessageRoute route, byte? sender = null) where TMessage : ModuleMessageHandler
    {
        return ModuleCreate(typeof(TMessage), buffer, route, sender);
    }

    public static NetMessage ModuleCreate(Type type, NetWriter writer, MessageRoute route, byte? sender = null)
    {
        return ModuleCreate(type, writer.Buffer, route, sender);
    }

    public static NetMessage ModuleCreate(Type type, ArraySegment<byte> buffer, MessageRoute route, byte? sender = null)
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
            Sender = sender,
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
        var message = Create(size);

        for (var i = 0; i < size; i++)
        {
            message._buffer[i] = writer.Buffer[i];
        }

        return message;
    }

    public byte[] ToByteArray()
    {
        var bytes = new byte[Length];

        Marshal.Copy((IntPtr)_buffer, bytes, 0, Length);

        return bytes;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        GC.SuppressFinalize(this);
        Marshal.FreeHGlobal((IntPtr)_buffer);

        _disposed = true;
    }
}