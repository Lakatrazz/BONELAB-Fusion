using LabFusion.Preferences;
using LabFusion.Representation;
using LabFusion.SDK.Gamemodes;

using SLZ.Props;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.Utilities {
    public static class FusionDevTools {
        public static bool DespawnDevTool(PlayerId id) {
            // Check gamemode
            if (Gamemode.ActiveGamemode != null) {
                var gamemode = Gamemode.ActiveGamemode;

                if (gamemode.DisableDevTools)
                    return true;
            }

            // Check permission level
            FusionPermissions.FetchPermissionLevel(id, out var level, out _);
            if (!FusionPermissions.HasSufficientPermissions(level, FusionPreferences.ActiveServerSettings.DevToolsAllowed.GetValue())) {
                return true;
            }

            return false;
        }

        public static bool PreventSpawnGun(PlayerId id) {
            // Check gamemode
            if (Gamemode.ActiveGamemode != null)
            {
                var gamemode = Gamemode.ActiveGamemode;

                if (gamemode.DisableSpawnGun)
                    return true;
            }

            // Check permission level
            FusionPermissions.FetchPermissionLevel(id, out var level, out _);
            if (!FusionPermissions.HasSufficientPermissions(level, FusionPreferences.ActiveServerSettings.DevToolsAllowed.GetValue())) {
                return true;
            }

            return false;
        }
    }
}
