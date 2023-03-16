using LabFusion.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
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


    public class FusionMessage : IDisposable
    {
        private byte[] buffer;

        public int Length
        {
            get
            {
                return buffer.Length;
            }
        }

        public byte[] Buffer
        {
            get
            {
                return buffer;
            }
        }

        public static FusionMessage Create(byte tag, FusionWriter writer) {
            writer.EnsureLength();

            return Create(tag, writer.Buffer);
        }

        public static FusionMessage Create(byte tag, byte[] buffer) {
            int length = buffer.Length;

            var message = new FusionMessage {
                buffer = ByteRetriever.Rent(length + 1)
            };
            message.buffer[0] = tag;

            System.Buffer.BlockCopy(buffer, 0, message.buffer, 1, length);

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
            writer.EnsureLength();

            return ModuleCreate(type, writer.Buffer);
        }

        public static FusionMessage ModuleCreate(Type type, byte[] buffer) {
            int length = buffer.Length;

            var message = new FusionMessage {
                buffer = ByteRetriever.Rent(length + 3)
            };
            message.buffer[0] = NativeMessageTag.Module;

            // Assign the module type
            byte[] typeBytes;
            var tag = ModuleMessageHandler.GetHandlerTag(type);

            // Make sure the tag is valid, otherwise we dont return a message
            if (tag.HasValue)
                typeBytes = BitConverter.GetBytes(tag.Value);
            else
                return null;

            message.Buffer[1] = typeBytes[0];
            message.Buffer[2] = typeBytes[1];

            System.Buffer.BlockCopy(buffer, 0, message.buffer, 3, length);

            return message;
        }

        public void Dispose() {
            GC.SuppressFinalize(this);

            ByteRetriever.Return(buffer);
        }
    }
}
