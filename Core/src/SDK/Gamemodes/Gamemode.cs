using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.SDK.Gamemodes {
    public abstract class Gamemode {
        public virtual bool CanLateJoin => true;
    }
}
