using LabFusion.BoneMenu;
using LabFusion.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;

namespace LabFusion.SDK.Gamemodes {
    public static class GamemodeManager {
        public static event Action<Gamemode> OnGamemodeChanged;

        internal static void Internal_OnFixedUpdate() {
            if (Gamemode.ActiveGamemode != null)
                Gamemode.ActiveGamemode.FixedUpdate();
        }

        internal static void Internal_OnUpdate()
        {
            if (Gamemode.ActiveGamemode != null)
                Gamemode.ActiveGamemode.Update();
        }

        internal static void Internal_OnLateUpdate()
        {
            if (Gamemode.ActiveGamemode != null)
                Gamemode.ActiveGamemode.LateUpdate();
        }

        internal static void Internal_SetActiveGamemode(Gamemode gamemode) {
            if (Gamemode._activeGamemode != gamemode) {
                Gamemode._activeGamemode = gamemode;
                OnGamemodeChanged.InvokeSafe(gamemode, "executing hook OnGamemodeChanged");
            }

            if (gamemode == null)
                BoneMenuCreator.SetActiveGamemodeText("No Active Gamemode");
            else
                BoneMenuCreator.SetActiveGamemodeText($"Stop {gamemode.GamemodeName}");
        }

        public static bool TryGetGamemode(ushort tag, out Gamemode gamemode) {
            // Gamemodes are null?
            if (Gamemodes == null) {
                FusionLogger.Error("While trying to find a Gamemode, the gamemode array was null!");
                gamemode = null;
                return false;
            }

            // Since gamemodes cannot be assumed to exist for everyone, we need to null check
            if (Gamemodes.Count > tag && Gamemodes.ElementAt(tag) != null) {
                gamemode = GamemodeRegistration.Gamemodes[tag];
                return true;
            }

            gamemode = null;
            return false;
        }

        public static Gamemode GetGamemode(ushort tag) {
            TryGetGamemode(tag, out var gamemode);
            return gamemode;
        }

        public static bool TryGetGamemode<TGamemode>(out TGamemode gamemode) where TGamemode : Gamemode {
            // Try find the gamemode from the type
            foreach (var other in Gamemodes) {
                if (other is TGamemode) {
                    gamemode = other as TGamemode;
                    return true;
                }
            }

            gamemode = null;
            return false;
        }

        public static IReadOnlyCollection<Gamemode> Gamemodes => GamemodeRegistration.Gamemodes;
    }
}
