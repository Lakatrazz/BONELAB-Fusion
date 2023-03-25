using LabFusion.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UIElements;

namespace LabFusion.Network
{
    public enum NetworkChannel : byte
    {
        Reliable,
        Unreliable,
        VoiceChat,
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

        internal static FusionMessage Internal_Create(int size) {
            return new FusionMessage() {
                _buffer = (byte*)Marshal.AllocHGlobal(size),
                _size = size,
                _disposed = false,
            };
        }

        public static FusionMessage Create(byte tag, FusionWriter writer) {
            return Create(tag, writer.Buffer, writer.Length);
        }

        public static FusionMessage Create(byte tag, byte[] buffer, int length = -1) {
            if (length <= 0)
                length = buffer.Length;

            int size = length + 1;
            var message = Internal_Create(size);

            message._buffer[0] = tag;
            for (var i = 0; i < length; i++) {
                message._buffer[i + 1] = buffer[i];
            }

            return message;
        }

        public static FusionMessage ModuleCreate<TMessage>(FusionWriter writer) where TMessage : ModuleMessageHandler {
            return ModuleCreate(typeof(TMessage), writer);
        }

        public static FusionMessage ModuleCreate<TMessage>(byte[] buffer) where TMessage : ModuleMessageHandler
        {
            return ModuleCreate(typeof(TMessage), buffer);
        }

        public static FusionMessage ModuleCreate(Type type, FusionWriter writer) {
            return ModuleCreate(type, writer.Buffer, writer.Length);
        }

        public static FusionMessage ModuleCreate(Type type, byte[] buffer, int length = -1) {
            if (length <= 0)
                length = buffer.Length;

            int size = length + 3;

            // Assign the module type
            var tag = ModuleMessageHandler.GetHandlerTag(type);

            // Make sure the tag is valid, otherwise we dont return a message
            if (tag.HasValue) {
                var value = tag.Value;

                var message = Internal_Create(size);
                message._buffer[0] = NativeMessageTag.Module;
                message._buffer[1] = (byte)(value >> 8);
                message._buffer[2] = (byte)value;

                for (var i = 0; i < length; i++) {
                    message._buffer[i + 3] = buffer[i];
                }

                return message;
            }
            else
                return null;
        }

        public void Dispose() {
            if (_disposed)
                return;

            GC.SuppressFinalize(this);
            Marshal.FreeHGlobal((IntPtr)_buffer);

            _disposed = true;
        }
    }
}
