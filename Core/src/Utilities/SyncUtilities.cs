using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Utilities {
    public static class SyncUtilities
    {
        public enum SyncGroup : byte {
            UNKNOWN = 0,
            PLAYER_BODY = 1,
            PROP = 2,
            NPC = 3,
        }
    }
}
