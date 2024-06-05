using LabFusion.Data;
using LabFusion.Patching;

namespace LabFusion.Network
{
    public enum ArenaMenuType
    {
        UNKNOWN = 0,
        CHALLENGE_SELECT = 1,
        TRIAL_SELECT = 2,
        SURVIVAL_SELECT = 3,
        TOGGLE_DIFFICULTY = 4,
        TOGGLE_ENEMY_PROFILE = 5,
        CREATE_CUSTOM_GAME_AND_START = 6,
        RESUME_SURVIVAL_FROM_ROUND = 7,
    }

    public class ArenaMenuData : IFusionSerializable
    {
        public byte selectionNumber;
        public ArenaMenuType type;

        public void Serialize(FusionWriter writer)
        {
            writer.Write(selectionNumber);
            writer.Write((byte)type);
        }

        public void Deserialize(FusionReader reader)
        {
            selectionNumber = reader.ReadByte();
            type = (ArenaMenuType)reader.ReadByte();
        }

        public static ArenaMenuData Create(byte selectionNumber, ArenaMenuType type)
        {
            return new ArenaMenuData()
            {
                selectionNumber = selectionNumber,
                type = type,
            };
        }
    }

    [Net.DelayWhileTargetLoading]
    public class ArenaMenuMessage : FusionMessageHandler
    {
        public override byte? Tag => NativeMessageTag.ArenaMenu;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
        {
            using FusionReader reader = FusionReader.Create(bytes);
            var data = reader.ReadFusionSerializable<ArenaMenuData>();
            var menu = ArenaData.MenuController;

            ArenaMenuPatches.IgnorePatches = true;

            // We ONLY handle this for clients, this message should only ever be sent by the server!
            if (!NetworkInfo.IsServer && menu)
            {
                switch (data.type)
                {
                    default:
                    case ArenaMenuType.UNKNOWN:
                        break;
                    case ArenaMenuType.CHALLENGE_SELECT:
                        menu.ChallengeSelect(data.selectionNumber);
                        break;
                    case ArenaMenuType.TRIAL_SELECT:
                        menu.TrialSelect(data.selectionNumber);
                        break;
                    case ArenaMenuType.SURVIVAL_SELECT:
                        menu.SurvivalSelect();
                        break;
                    case ArenaMenuType.TOGGLE_DIFFICULTY:
                        menu.ToggleDifficulty(data.selectionNumber);
                        break;
                    case ArenaMenuType.TOGGLE_ENEMY_PROFILE:
                        menu.ToggleEnemyProfile(data.selectionNumber);
                        break;
                    case ArenaMenuType.CREATE_CUSTOM_GAME_AND_START:
                        menu.CreateCustomGameAndStart();
                        break;
                    case ArenaMenuType.RESUME_SURVIVAL_FROM_ROUND:
                        menu.ResumeSurvivalFromRound();
                        break;
                }
            }

            ArenaMenuPatches.IgnorePatches = false;
        }
    }
}
