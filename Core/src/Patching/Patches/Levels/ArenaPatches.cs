using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;
using Il2CppSystem;
using Il2CppSystem.Linq;
using LabFusion.Data;
using LabFusion.Network;
using LabFusion.Senders;
using LabFusion.Utilities;

using SLZ.Bonelab;

namespace LabFusion.Patching {
    [HarmonyPatch(typeof(GeoManager))]
    public static class GeoManagerPatches {
        public static bool IgnorePatches = false;

        [HarmonyPrefix]
        [HarmonyPatch(nameof(GeoManager.ToggleGeo))]
        public static bool ToggleGeo(GeoManager __instance, int index) {
            return IgnorePatches || QuickSender.SendServerMessage(() => {
                ArenaSender.SendGeometryChange((byte)index);
            });
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
            return IgnorePatches || QuickSender.SendServerMessage(() => {
                ArenaSender.SendMenuSelection((byte)sel, ArenaMenuType.CHALLENGE_SELECT);
            });
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(ArenaMenuController.TrialSelect))]
        public static bool TrialSelect(ArenaMenuController __instance, int sel)
        {
            return IgnorePatches || QuickSender.SendServerMessage(() => {
                ArenaSender.SendMenuSelection((byte)sel, ArenaMenuType.TRIAL_SELECT);
            });
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(ArenaMenuController.SurvivalSelect))]
        public static bool SurvivalSelect(ArenaMenuController __instance)
        {
            return IgnorePatches || QuickSender.SendServerMessage(() => {
                ArenaSender.SendMenuSelection(0, ArenaMenuType.SURVIVAL_SELECT);
            });
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(ArenaMenuController.ToggleDifficulty))]
        public static bool ToggleDifficulty(ArenaMenuController __instance, int diff)
        {
            return IgnorePatches || QuickSender.SendServerMessage(() => {
                ArenaSender.SendMenuSelection((byte)diff, ArenaMenuType.TOGGLE_DIFFICULTY);
            });
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(ArenaMenuController.ToggleEnemyProfile))]
        public static bool ToggleEnemyProfile(ArenaMenuController __instance, int profileIndex)
        {
            return IgnorePatches || QuickSender.SendServerMessage(() => {
                ArenaSender.SendMenuSelection((byte)profileIndex, ArenaMenuType.TOGGLE_ENEMY_PROFILE);
            });
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(ArenaMenuController.CreateCustomGameAndStart))]
        public static bool CreateCustomGameAndStart(ArenaMenuController __instance)
        {
            return IgnorePatches || QuickSender.SendServerMessage(() => {
                ArenaSender.SendMenuSelection(0, ArenaMenuType.CREATE_CUSTOM_GAME_AND_START);
            });
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(ArenaMenuController.ResumeSurvivalFromRound))]
        public static bool ResumeSurvivalFromRound(ArenaMenuController __instance)
        {
            return IgnorePatches || QuickSender.SendServerMessage(() => {
                ArenaSender.SendMenuSelection(0, ArenaMenuType.RESUME_SURVIVAL_FROM_ROUND);
            });
        }
    }

    [HarmonyPatch(typeof(ChallengeSelectMenu))]
    public static class ChallengePatches {
        public static bool IgnorePatches = false;

        [HarmonyPrefix]
        [HarmonyPatch(nameof(ChallengeSelectMenu.OnChallengeSelect))]
        public static bool OnChallengeSelect(ChallengeSelectMenu __instance)
        {
            return IgnorePatches || QuickSender.SendServerMessage(() => {
                ArenaSender.SendChallengeSelect(ArenaData.GetIndex(__instance).Value, 0, ChallengeSelectType.ON_CHALLENGE_SELECT);
            });
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(ChallengeSelectMenu.SelectChallenge))]
        public static bool SelectChallenge(ChallengeSelectMenu __instance, int idx) {
            return IgnorePatches || QuickSender.SendServerMessage(() => {
                ArenaSender.SendChallengeSelect(ArenaData.GetIndex(__instance).Value, (byte)idx, ChallengeSelectType.SELECT_CHALLENGE);
            });
        }
    }

    [HarmonyPatch(typeof(Arena_GameController))]
    public static class ArenaPatches {
        public static bool IgnorePatches = false;

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Arena_GameController.ARENA_PlayerEnter))]
        public static bool ARENA_PlayerEnter()
        {
            return IgnorePatches || QuickSender.SendServerMessage(() => {
                ArenaSender.SendArenaTransition(ArenaTransitionType.ARENA_PLAYER_ENTER);
            });
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Arena_GameController.InitObjectiveContainer))]
        public static bool InitObjectiveContainer()
        {
            return IgnorePatches || QuickSender.SendServerMessage(() => {
                ArenaSender.SendArenaTransition(ArenaTransitionType.INIT_OBJECTIVE_CONTAINER);
            });
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Arena_GameController.ARENA_StartMatch))]
        public static bool ARENA_StartMatch()
        {
            return IgnorePatches || QuickSender.SendServerMessage(() => {
                ArenaSender.SendArenaTransition(ArenaTransitionType.ARENA_START_MATCH);
            });
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Arena_GameController.StartNextWave))]
        public static bool StartNextWave() {
            return IgnorePatches || QuickSender.SendServerMessage(() => {
                ArenaSender.SendArenaTransition(ArenaTransitionType.START_NEXT_WAVE);
            });
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Arena_GameController.ARENA_QuitChallenge))]
        public static bool ARENA_QuitChallenge()
        {
            return IgnorePatches || QuickSender.SendServerMessage(() => {
                ArenaSender.SendArenaTransition(ArenaTransitionType.ARENA_QUIT_CHALLENGE);
            });
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Arena_GameController.ARENA_CancelMatch))]
        public static bool ARENA_CancelMatch()
        {
            return IgnorePatches || QuickSender.SendServerMessage(() => {
                ArenaSender.SendArenaTransition(ArenaTransitionType.ARENA_CANCEL_MATCH);
            });
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Arena_GameController.ARENA_ResetTheBell))]
        public static bool ARENA_ResetTheBell()
        {
            return IgnorePatches || QuickSender.SendServerMessage(() => {
                ArenaSender.SendArenaTransition(ArenaTransitionType.ARENA_RESET_THE_BELL);
            });
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Arena_GameController.ARENA_RingTheBell))]
        public static bool ARENA_RingTheBell()
        {
            return IgnorePatches || QuickSender.SendServerMessage(() => {
                ArenaSender.SendArenaTransition(ArenaTransitionType.ARENA_RING_THE_BELL);
            });
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Arena_GameController.FailObjectiveMode))]
        public static bool FailObjectiveMode()
        {
            return IgnorePatches || QuickSender.SendServerMessage(() => {
                ArenaSender.SendArenaTransition(ArenaTransitionType.FAIL_OBJECTIVE_MODE);
            });
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Arena_GameController.FailEscapeMode))]
        public static bool FailEscapeMode()
        {
            return IgnorePatches || QuickSender.SendServerMessage(() => {
                ArenaSender.SendArenaTransition(ArenaTransitionType.FAIL_ESCAPE_MODE);
            });
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Arena_GameController.SpawnLoot))]
        public static bool SpawnLoot()
        {
            return IgnorePatches || QuickSender.SendServerMessage(() => {
                ArenaSender.SendArenaTransition(ArenaTransitionType.SPAWN_LOOT);
            });
        }
    }
}
