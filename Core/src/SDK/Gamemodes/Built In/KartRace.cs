using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.SDK.Gamemodes {
    public class KartRace : Gamemode {
        public override string GamemodeCategory => "Fusion";
        public override string GamemodeName => "Kart Race";

        public override bool VisibleInBonemenu => false;
    }
}
