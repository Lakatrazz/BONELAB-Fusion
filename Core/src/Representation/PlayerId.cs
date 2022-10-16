using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LabFusion.Data;
using LabFusion.Network;

namespace LabFusion.Representation
{
    public class PlayerId : IFusionSerializable, IDisposable {
        public ulong LongId { get; private set; }
        public byte SmallId { get; private set; }
        public string Username { get; private set; }

        public PlayerId() { }

        public PlayerId(ulong longId, byte smallId, string username) {
            LongId = longId;
            SmallId = smallId;
            Username = username;
        }

        public void Insert() {
            PlayerIdManager.PlayerIds.Add(this);
        }

        public void Dispose() {
            PlayerIdManager.PlayerIds.Remove(this);
            if (PlayerIdManager.LocalId == this)
                PlayerIdManager.RemoveLocalId();

            GC.SuppressFinalize(this);
        }

        public void Serialize(FusionWriter writer) {
            writer.Write(LongId);
            writer.Write(SmallId);
            writer.Write(Username);
        }
        
        public void Deserialize(FusionReader reader) {
            LongId = reader.ReadUInt64();
            SmallId = reader.ReadByte();
            Username = reader.ReadString();
        }
    }
}
