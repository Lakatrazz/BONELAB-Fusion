using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.SDK.Gamemodes {
    public class BoneBall : Gamemode {
        public override string GamemodeCategory => "Fusion";
        public override string GamemodeName => "Bone Ball";

        public override bool VisibleInBonemenu => false;
    }
}
