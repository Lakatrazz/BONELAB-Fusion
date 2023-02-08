using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.SDK.Gamemodes {
    public static class GamemodeManager {
        private static Gamemode _activeGamemode;

        public static Gamemode ActiveGamemode => _activeGamemode;
    }
}
