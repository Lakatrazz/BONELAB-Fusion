using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Representation
{
    public class PlayerRep : IDisposable {
        public static readonly Dictionary<byte, PlayerRep> Representations = new Dictionary<byte, PlayerRep>();

        public PlayerId PlayerId { get; private set; }

        public PlayerRep(PlayerId playerId)
        {
            PlayerId = playerId;
            Representations.Add(playerId.SmallId, this);
        }

        public void OnCreateRepresentations() {

        }

        public void Dispose() {
            Representations.Remove(PlayerId.SmallId);
        }
    }
}
