using LabFusion.Network.Serialization;

using LabFusion.SDK.Modules;
using LabFusion.Utilities;
using System.Runtime.InteropServices;

namespace LabFusion.Network;

public enum NetworkChannel : byte
{
    Reliable,
    Unreliable,
}

public class MessagePrefix : INetSerializable
{
    public byte Tag;
    public RelayType Type;
    public NetworkChannel Channel;
    public byte? Sender = null;
    public byte? Target = null;

    public int? GetSize()
    {
        return sizeof(byte) * 7;
    }

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref Tag);
        serializer.SerializeValue(ref Type, Precision.OneByte);
        serializer.SerializeValue(ref Channel, Precision.OneByte);

        if (Type != RelayType.None)
        {
            serializer.SerializeValue(ref Sender);
        }

        if (Type == RelayType.ToTarget)
        {
            serializer.SerializeValue(ref Target);
        }
    }
}

public unsafe class FusionMessage : IDisposable
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

    private static FusionMessage Create(int size)
    {
        return new FusionMessage()
        {
            _buffer = (byte*)Marshal.AllocHGlobal(size),
            _size = size,
            _disposed = false,
        };
    }

    public static FusionMessage Create(byte tag, NetWriter writer, RelayType relayType = RelayType.None, NetworkChannel channel = NetworkChannel.Reliable, byte? sender = null, byte? target = null)
    {
        return Create(tag, writer.Buffer, relayType, channel, sender, target);
    }

    public static FusionMessage Create(byte tag, ArraySegment<byte> buffer, RelayType relayType = RelayType.None, NetworkChannel channel = NetworkChannel.Reliable, byte? sender = null, byte? target = null)
    {
        var prefix = new MessagePrefix()
        {
            Tag = tag,
            Type = relayType,
            Channel = channel,
            Sender = sender,
            Target = target,
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

    public static FusionMessage Create(byte tag, ReceivedMessage received)
    {
        var prefix = new MessagePrefix()
        {
            Tag = tag,
            Type = received.Type,
            Channel = received.Channel,
            Sender = received.Sender,
            Target = received.Target,
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

    public static FusionMessage ModuleCreate<TMessage>(NetWriter writer, RelayType relayType = RelayType.None, NetworkChannel channel = NetworkChannel.Reliable, byte? sender = null, byte? target = null) where TMessage : ModuleMessageHandler
    {
        return ModuleCreate(typeof(TMessage), writer, relayType, channel, sender, target);
    }

    public static FusionMessage ModuleCreate<TMessage>(byte[] buffer, RelayType relayType = RelayType.None, NetworkChannel channel = NetworkChannel.Reliable, byte? sender = null, byte? target = null) where TMessage : ModuleMessageHandler
    {
        return ModuleCreate(typeof(TMessage), buffer, relayType, channel, sender, target);
    }

    public static FusionMessage ModuleCreate(Type type, NetWriter writer, RelayType relayType = RelayType.None, NetworkChannel channel = NetworkChannel.Reliable, byte? sender = null, byte? target = null)
    {
        return ModuleCreate(type, writer.Buffer, relayType, channel, sender, target);
    }

    public static FusionMessage ModuleCreate(Type type, ArraySegment<byte> buffer, RelayType relayType = RelayType.None, NetworkChannel channel = NetworkChannel.Reliable, byte? sender = null, byte? target = null)
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
            Type = relayType,
            Channel = channel,
            Sender = sender,
            Target = target,
        };

        using var writer = NetWriter.Create(prefix.GetSize().Value + buffer.Count + sizeof(long) + sizeof(int));

        writer.SerializeValue(ref prefix);

        var expandedBuffer = new byte[buffer.Count + sizeof(long)];

        BigEndianHelper.WriteBytes(expandedBuffer, 0, value);

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