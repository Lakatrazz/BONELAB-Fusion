using LabFusion.SDK.Modules;
using LabFusion.Utilities;

using System.Runtime.InteropServices;

namespace LabFusion.Network;

public enum NetworkChannel : byte
{
    Reliable,
    Unreliable,
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

    public static FusionMessage Create(byte tag, FusionWriter writer)
    {
        return Create(tag, writer.Buffer, writer.Length);
    }

    public static FusionMessage Create(byte tag, byte[] buffer, int length = -1)
    {
        if (length <= 0)
        {
            length = buffer.Length;
        }

        int size = length + 1;
        var message = Create(size);

        message._buffer[0] = tag;
        for (var i = 0; i < length; i++)
        {
            message._buffer[i + 1] = buffer[i];
        }

        return message;
    }

    public static FusionMessage ModuleCreate<TMessage>(FusionWriter writer) where TMessage : ModuleMessageHandler
    {
        return ModuleCreate(typeof(TMessage), writer);
    }

    public static FusionMessage ModuleCreate<TMessage>(byte[] buffer) where TMessage : ModuleMessageHandler
    {
        return ModuleCreate(typeof(TMessage), buffer);
    }

    public static FusionMessage ModuleCreate(Type type, FusionWriter writer)
    {
        return ModuleCreate(type, writer.Buffer, writer.Length);
    }

    public static FusionMessage ModuleCreate(Type type, byte[] buffer, int length = -1)
    {
        if (length <= 0)
        {
            length = buffer.Length;
        }

        int size = length + 3;

        // Assign the module type
        var tag = ModuleMessageHandler.GetHandlerTag(type);

        // Make sure the tag is valid, otherwise we dont return a message
        if (tag.HasValue)
        {
            var value = tag.Value;
            var tagBytes = BitConverter.GetBytes((ushort)value);

            var message = Create(size);
            message._buffer[0] = NativeMessageTag.Module;
            message._buffer[1] = tagBytes[0];
            message._buffer[2] = tagBytes[1];

            for (var i = 0; i < length; i++)
            {
                message._buffer[i + 3] = buffer[i];
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
        byte[] bytes = ByteRetriever.Rent(Length);
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