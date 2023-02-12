using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.SDK.Gamemodes {
    public class TugOWar : Gamemode {
        public override string GamemodeCategory => "Fusion";
        public override string GamemodeName => "Tug O' War";

        public override bool VisibleInBonemenu => false;
    }
}
