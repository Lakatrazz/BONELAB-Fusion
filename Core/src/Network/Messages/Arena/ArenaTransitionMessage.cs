using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LabFusion.Data;
using LabFusion.Representation;
using LabFusion.Utilities;
using LabFusion.Grabbables;
using LabFusion.Syncables;
using LabFusion.Patching;

using SLZ;
using SLZ.Interaction;
using SLZ.Props.Weapons;

namespace LabFusion.Network
{
    public enum ArenaTransitionType {
        UNKNOWN = 0,
        ARENA_PLAYER_ENTER = 1,
        INIT_OBJECTIVE_CONTAINER = 2,
        ARENA_START_MATCH = 3,
        START_NEXT_WAVE = 4,
        ARENA_QUIT_CHALLENGE = 5,
        ARENA_CANCEL_MATCH = 6,
        ARENA_RESET_THE_BELL = 7,
        ARENA_RING_THE_BELL = 8,
        FAIL_OBJECTIVE_MODE = 9,
        FAIL_ESCAPE_MODE = 10,
        SPAWN_LOOT = 11,
    }

    public class ArenaTransitionData : IFusionSerializable, IDisposable
    {
        public ArenaTransitionType type;

        public void Serialize(FusionWriter writer)
        {
            writer.Write((byte)type);
        }

        public void Deserialize(FusionReader reader)
        {
            type = (ArenaTransitionType)reader.ReadByte();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public static ArenaTransitionData Create(ArenaTransitionType type)
        {
            return new ArenaTransitionData()
            {
                type = type,
            };
        }
    }

    [Net.DelayWhileTargetLoading]
    public class ArenaTransitionMessage : FusionMessageHandler
    {
        public override byte? Tag => NativeMessageTag.ArenaTransition;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
        {
            using (FusionReader reader = FusionReader.Create(bytes))
            {
                using (var data = reader.ReadFusionSerializable<ArenaTransitionData>())
                {
                    ArenaPatches.IgnorePatches = true;

                    // We ONLY handle this for clients, this message should only ever be sent by the server!
                    if (!NetworkInfo.IsServer && ArenaData.IsInArena) {
                        switch (data.type) {
                            default:
                            case ArenaTransitionType.UNKNOWN:
                                break;
                            case ArenaTransitionType.ARENA_PLAYER_ENTER:
                                ArenaData.GameController.ARENA_PlayerEnter();
                                break;
                            case ArenaTransitionType.INIT_OBJECTIVE_CONTAINER:
                                ArenaData.GameController.InitObjectiveContainer();

                                if (ArenaData.GameControlDisplay)
                                    ArenaData.GameControlDisplay.gameObject.SetActive(true);
                                break;
                            case ArenaTransitionType.ARENA_START_MATCH:
                                ArenaData.GameController.ARENA_StartMatch();
                                break;
                            case ArenaTransitionType.START_NEXT_WAVE:
                                ArenaData.GameController.StartNextWave();
                                break;
                            case ArenaTransitionType.ARENA_QUIT_CHALLENGE:
                                ArenaData.GameController.ARENA_QuitChallenge();
                                break;
                            case ArenaTransitionType.ARENA_CANCEL_MATCH:
                                ArenaData.GameController.ARENA_CancelMatch();
                                break;
                            case ArenaTransitionType.ARENA_RESET_THE_BELL:
                                ArenaData.GameController.ARENA_ResetTheBell();
                                break;
                            case ArenaTransitionType.ARENA_RING_THE_BELL:
                                ArenaData.GameController.ARENA_RingTheBell();
                                break;
                            case ArenaTransitionType.FAIL_OBJECTIVE_MODE:
                                ArenaData.GameController.FailObjectiveMode();
                                break;
                            case ArenaTransitionType.FAIL_ESCAPE_MODE:
                                ArenaData.GameController.FailEscapeMode();
                                break;
                            case ArenaTransitionType.SPAWN_LOOT:
                                ArenaData.GameController.SpawnLoot();
                                break;
                        }
                    }

                    ArenaPatches.IgnorePatches = false;
                }
            }
        }
    }
}
