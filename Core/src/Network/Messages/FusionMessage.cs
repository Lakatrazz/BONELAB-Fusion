using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Network
{
    public enum NetworkChannel : byte
    {
        Reliable,
        Unreliable,
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
            return Create(tag, writer.Buffer);
        }

        public static FusionMessage Create(byte tag, byte[] buffer) {
            var message = new FusionMessage {
                buffer = new byte[buffer.Length + 1]
            };
            message.buffer[0] = tag;
            buffer.CopyTo(message.buffer, 1);
            return message;
        }

        public void Dispose() {
            GC.SuppressFinalize(this);
        }
    }
}
