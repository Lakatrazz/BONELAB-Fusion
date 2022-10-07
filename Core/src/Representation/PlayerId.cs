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
        public static readonly List<PlayerId> PlayerIds = new List<PlayerId>();

        public static ulong ConstantLongId { get; private set; }
        public static PlayerId SelfId { get; private set; }

        public ulong LongId { get; private set; }
        public byte SmallId { get; private set; }

        public static byte? GetUnusedPlayerId() {
            for (byte i = 0; i < 255; i++) {
                if (GetPlayerId(i) == null)
                    return i;
            }
            return null;
        }

        public static PlayerId GetPlayerId(byte smallId) {
            return PlayerIds.FirstOrDefault(x => x.SmallId == smallId);
        }

        public static PlayerId GetPlayerId(ulong longId) {
            return PlayerIds.FirstOrDefault(x => x.LongId == longId);
        }

        public static void UpdateSelfId() {
            var id = GetPlayerId(ConstantLongId);
            if (id != null)
                SelfId = id;
            else
                SelfId = null;
        }

        public static void SetConstantId(ulong longId) {
            ConstantLongId = longId;
        }

        public PlayerId() { }

        public PlayerId(ulong longId, byte smallId) {
            LongId = longId;
            SmallId = smallId;
        }

        public void Insert() {
            PlayerIds.Add(this);
        }

        public void Dispose() {
            PlayerIds.Remove(this);
            if (SelfId == this)
                SelfId = null;
        }

        public void Serialize(FusionWriter writer) {
            writer.Write(LongId);
            writer.Write(SmallId);
        }
        
        public void Deserialize(FusionReader reader) {
            LongId = reader.ReadUInt64();
            SmallId = reader.ReadByte();
        }
    }
}
