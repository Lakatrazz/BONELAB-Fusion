using LabFusion.Data;
using LabFusion.SDK.Modules;

using System.Runtime.InteropServices;

namespace LabFusion.Network;

public enum NetworkChannel : byte
{
    Reliable,
    Unreliable,
}

public class MessagePrefix : IFusionSerializable
{
    public byte Tag { get; set; }
    public RelayType Type { get; set; }
    public NetworkChannel Channel { get; set; }
    public byte? Sender { get; set; } = null;
    public byte? Target { get; set; } = null;

    public void Serialize(FusionWriter writer)
    {
        writer.Write(Tag);
        writer.Write((byte)Type);
        writer.Write((byte)Channel);

        if (Type != RelayType.None)
        {
            writer.Write(Sender.Value);
        }

        if (Type == RelayType.ToTarget)
        {
            writer.Write(Target.Value);
        }
    }

    public void Deserialize(FusionReader reader)
    {
        Tag = reader.ReadByte();
        Type = (RelayType)reader.ReadByte();
        Channel = (NetworkChannel)reader.ReadByte();

        if (Type != RelayType.None)
        {
            Sender = reader.ReadByte();
        }

        if (Type == RelayType.ToTarget)
        {
            Target = reader.ReadByte();
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

    public static FusionMessage Create(byte tag, FusionWriter writer, RelayType relayType = RelayType.None, NetworkChannel channel = NetworkChannel.Reliable, byte? sender = null, byte? target = null)
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

        using var writer = FusionWriter.Create();

        writer.Write(prefix);
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

        using var writer = FusionWriter.Create();

        writer.Write(prefix);
        writer.Write(received.Bytes);

        int size = writer.Length;
        var message = Create(size);

        for (var i = 0; i < size; i++)
        {
            message._buffer[i] = writer.Buffer[i];
        }

        return message;
    }

    public static FusionMessage ModuleCreate<TMessage>(FusionWriter writer, RelayType relayType = RelayType.None, NetworkChannel channel = NetworkChannel.Reliable, byte? sender = null, byte? target = null) where TMessage : ModuleMessageHandler
    {
        return ModuleCreate(typeof(TMessage), writer, relayType, channel, sender, target);
    }

    public static FusionMessage ModuleCreate<TMessage>(byte[] buffer, RelayType relayType = RelayType.None, NetworkChannel channel = NetworkChannel.Reliable, byte? sender = null, byte? target = null) where TMessage : ModuleMessageHandler
    {
        return ModuleCreate(typeof(TMessage), buffer, relayType, channel, sender, target);
    }

    public static FusionMessage ModuleCreate(Type type, FusionWriter writer, RelayType relayType = RelayType.None, NetworkChannel channel = NetworkChannel.Reliable, byte? sender = null, byte? target = null)
    {
        return ModuleCreate(type, writer.Buffer, relayType, channel, sender, target);
    }

    public static FusionMessage ModuleCreate(Type type, ArraySegment<byte> buffer, RelayType relayType = RelayType.None, NetworkChannel channel = NetworkChannel.Reliable, byte? sender = null, byte? target = null)
    {
        // Assign the module type
        var tag = ModuleMessageHandler.GetHandlerTag(type);

        // Make sure the tag is valid, otherwise we dont return a message
        if (tag.HasValue)
        {
            var value = tag.Value;
            var tagBytes = BitConverter.GetBytes((ushort)value);

            var prefix = new MessagePrefix()
            {
                Tag = NativeMessageTag.Module,
                Type = relayType,
                Channel = channel,
                Sender = sender,
                Target = target,
            };

            using var writer = FusionWriter.Create();

            writer.Write(prefix);

            var expandedBuffer = new byte[buffer.Count + 2];
            expandedBuffer[0] = tagBytes[0];
            expandedBuffer[1] = tagBytes[1];

            for (var i = 0; i < buffer.Count; i++)
            {
                expandedBuffer[i + 2] = buffer[i];
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
        else
        {
            return null;
        }
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