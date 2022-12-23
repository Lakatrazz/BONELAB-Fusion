using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;
using Il2CppSystem;
using LabFusion.Data;
using LabFusion.Network;
using LabFusion.Utilities;

using SLZ.Bonelab;

namespace LabFusion.Patching {
    [HarmonyPatch(typeof(GeoManager))]
    public static class GeoManagerPatches {
        public static bool IgnorePatches = false;

        [HarmonyPrefix]
        [HarmonyPatch(nameof(GeoManager.ToggleGeo))]
        public static bool ToggleGeo(GeoManager __instance, int index) {
            if (IgnorePatches)
                return true;

            if (NetworkInfo.HasServer)
            {
                if (!NetworkInfo.IsServer)
                    return false;
                else
                {
                    ArenaData.TEMP_SendGeo((byte)index);
                }
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(GeoManager.ClearCurrentGeo))]
        public static bool ClearCurrentGeo(GeoManager __instance) {
            if (IgnorePatches)
                return true;

            if (NetworkInfo.HasServer && !NetworkInfo.IsServer) {
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(ArenaMenuController))]
    public static class ArenaMenuPatches {
        public static bool IgnorePatches = false;

        [HarmonyPrefix]
        [HarmonyPatch(nameof(ArenaMenuController.ChallengeSelect))]
        public static bool ChallengeSelect(ArenaMenuController __instance, int sel)
        {
            if (IgnorePatches)
                return true;

            if (NetworkInfo.HasServer)
            {
                if (!NetworkInfo.IsServer)
                    return false;
                else
                {
                    ArenaData.TEMP_SendMenuController((byte)sel, ArenaMenuType.CHALLENGE_SELECT);
                }
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(ArenaMenuController.TrialSelect))]
        public static bool TrialSelect(ArenaMenuController __instance, int sel)
        {
            if (IgnorePatches)
                return true;

            if (NetworkInfo.HasServer)
            {
                if (!NetworkInfo.IsServer)
                    return false;
                else
                {
                    ArenaData.TEMP_SendMenuController((byte)sel, ArenaMenuType.TRIAL_SELECT);
                }
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(ArenaMenuController.SurvivalSelect))]
        public static bool SurvivalSelect(ArenaMenuController __instance)
        {
            if (IgnorePatches)
                return true;

            if (NetworkInfo.HasServer)
            {
                if (!NetworkInfo.IsServer)
                    return false;
                else
                {
                    ArenaData.TEMP_SendMenuController(0, ArenaMenuType.SURVIVAL_SELECT);
                }
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(ArenaMenuController.ToggleDifficulty))]
        public static bool ToggleDifficulty(ArenaMenuController __instance, int diff)
        {
            if (IgnorePatches)
                return true;

            if (NetworkInfo.HasServer)
            {
                if (!NetworkInfo.IsServer)
                    return false;
                else
                {
                    ArenaData.TEMP_SendMenuController((byte)diff, ArenaMenuType.TOGGLE_DIFFICULTY);
                }
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(ArenaMenuController.ToggleEnemyProfile))]
        public static bool ToggleEnemyProfile(ArenaMenuController __instance, int profileIndex)
        {
            if (IgnorePatches)
                return true;

            if (NetworkInfo.HasServer)
            {
                if (!NetworkInfo.IsServer)
                    return false;
                else
                {
                    ArenaData.TEMP_SendMenuController((byte)profileIndex, ArenaMenuType.TOGGLE_ENEMY_PROFILE);
                }
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(ArenaMenuController.CreateCustomGameAndStart))]
        public static bool CreateCustomGameAndStart(ArenaMenuController __instance)
        {
            if (IgnorePatches)
                return true;

            if (NetworkInfo.HasServer)
            {
                if (!NetworkInfo.IsServer)
                    return false;
                else
                {
                    ArenaData.TEMP_SendMenuController(0, ArenaMenuType.CREATE_CUSTOM_GAME_AND_START);
                }
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(ArenaMenuController.ResumeSurvivalFromRound))]
        public static bool ResumeSurvivalFromRound(ArenaMenuController __instance)
        {
            if (IgnorePatches)
                return true;

            if (NetworkInfo.HasServer)
            {
                if (!NetworkInfo.IsServer)
                    return false;
                else
                {
                    ArenaData.TEMP_SendMenuController(0, ArenaMenuType.RESUME_SURVIVAL_FROM_ROUND);
                }
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(ChallengeSelectMenu))]
    public static class ChallengePatches {
        public static bool IgnorePatches = false;

        [HarmonyPrefix]
        [HarmonyPatch(nameof(ChallengeSelectMenu.OnChallengeSelect))]
        public static bool OnChallengeSelect(ChallengeSelectMenu __instance)
        {
            if (IgnorePatches)
                return true;

            if (NetworkInfo.HasServer)
            {
                if (!NetworkInfo.IsServer)
                    return false;
                else
                {
                    ArenaData.TEMP_SendChallengeSelect(ArenaData.GetIndex(__instance).Value, 0, ChallengeSelectType.ON_CHALLENGE_SELECT);
                }
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(ChallengeSelectMenu.SelectChallenge))]
        public static bool SelectChallenge(ChallengeSelectMenu __instance, int idx) {
            if (IgnorePatches)
                return true;

            if (NetworkInfo.HasServer) {
                if (!NetworkInfo.IsServer)
                    return false;
                else
                {
                    ArenaData.TEMP_SendChallengeSelect(ArenaData.GetIndex(__instance).Value, (byte)idx, ChallengeSelectType.SELECT_CHALLENGE);
                }
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(Arena_GameController))]
    public static class ArenaPatches {
        public static bool IgnorePatches = false;

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Arena_GameController.ARENA_PlayerEnter))]
        public static bool ARENA_PlayerEnter()
        {
            if (IgnorePatches)
                return true;

            if (NetworkInfo.HasServer)
            {
                if (!NetworkInfo.IsServer)
                    return false;
                else
                {
                    ArenaData.TEMP_SendArenaMessage(ArenaTransitionType.ARENA_PLAYER_ENTER);
                }
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Arena_GameController.InitObjectiveContainer))]
        public static bool InitObjectiveContainer()
        {
            if (IgnorePatches)
                return true;

            if (NetworkInfo.HasServer)
            {
                if (!NetworkInfo.IsServer)
                    return false;
                else
                {
                    ArenaData.TEMP_SendArenaMessage(ArenaTransitionType.INIT_OBJECTIVE_CONTAINER);
                }
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Arena_GameController.ARENA_StartMatch))]
        public static bool ARENA_StartMatch()
        {
            if (IgnorePatches)
                return true;

            if (NetworkInfo.HasServer)
            {
                if (!NetworkInfo.IsServer)
                    return false;
                else
                {
                    ArenaData.TEMP_SendArenaMessage(ArenaTransitionType.ARENA_START_MATCH);
                }
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Arena_GameController.StartNextWave))]
        public static bool StartNextWave() {
            if (IgnorePatches)
                return true;

            if (NetworkInfo.HasServer) {
                if (!NetworkInfo.IsServer)
                    return false;
                else {
                    ArenaData.TEMP_SendArenaMessage(ArenaTransitionType.START_NEXT_WAVE);
                }
            }
            
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Arena_GameController.ARENA_QuitChallenge))]
        public static bool ARENA_QuitChallenge()
        {
            if (IgnorePatches)
                return true;

            if (NetworkInfo.HasServer)
            {
                if (!NetworkInfo.IsServer)
                    return false;
                else
                {
                    ArenaData.TEMP_SendArenaMessage(ArenaTransitionType.ARENA_QUIT_CHALLENGE);
                }
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Arena_GameController.ARENA_CancelMatch))]
        public static bool ARENA_CancelMatch()
        {
            if (IgnorePatches)
                return true;

            if (NetworkInfo.HasServer)
            {
                if (!NetworkInfo.IsServer)
                    return false;
                else
                {
                    ArenaData.TEMP_SendArenaMessage(ArenaTransitionType.ARENA_CANCEL_MATCH);
                }
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Arena_GameController.ARENA_ResetTheBell))]
        public static bool ARENA_ResetTheBell()
        {
            if (IgnorePatches)
                return true;

            if (NetworkInfo.HasServer)
            {
                if (!NetworkInfo.IsServer)
                    return false;
                else
                {
                    ArenaData.TEMP_SendArenaMessage(ArenaTransitionType.ARENA_RESET_THE_BELL);
                }
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Arena_GameController.ARENA_RingTheBell))]
        public static bool ARENA_RingTheBell()
        {
            if (IgnorePatches)
                return true;

            if (NetworkInfo.HasServer)
            {
                if (!NetworkInfo.IsServer)
                    return false;
                else
                {
                    ArenaData.TEMP_SendArenaMessage(ArenaTransitionType.ARENA_RING_THE_BELL);
                }
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Arena_GameController.FailObjectiveMode))]
        public static bool FailObjectiveMode()
        {
            if (IgnorePatches)
                return true;

            if (NetworkInfo.HasServer)
            {
                if (!NetworkInfo.IsServer)
                    return false;
                else
                {
                    ArenaData.TEMP_SendArenaMessage(ArenaTransitionType.FAIL_OBJECTIVE_MODE);
                }
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Arena_GameController.FailEscapeMode))]
        public static bool FailEscapeMode()
        {
            if (IgnorePatches)
                return true;

            if (NetworkInfo.HasServer)
            {
                if (!NetworkInfo.IsServer)
                    return false;
                else
                {
                    ArenaData.TEMP_SendArenaMessage(ArenaTransitionType.FAIL_ESCAPE_MODE);
                }
            }

            return true;
        }
    }
}
