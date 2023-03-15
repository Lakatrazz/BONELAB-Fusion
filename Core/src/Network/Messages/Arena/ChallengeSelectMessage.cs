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
    public enum ChallengeSelectType {
        UNKNOWN = 0,
        SELECT_CHALLENGE = 1,
        ON_CHALLENGE_SELECT = 2,
    }

    public class ChallengeSelectData : IFusionSerializable, IDisposable
    {
        public byte menuIndex;
        public byte challengeNumber;
        public ChallengeSelectType type;

        public void Serialize(FusionWriter writer)
        {
            writer.Write(menuIndex);
            writer.Write(challengeNumber);
            writer.Write((byte)type);
        }

        public void Deserialize(FusionReader reader)
        {
            menuIndex = reader.ReadByte();
            challengeNumber = reader.ReadByte();
            type = (ChallengeSelectType)reader.ReadByte();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public static ChallengeSelectData Create(byte menuIndex, byte challengeNumber, ChallengeSelectType type)
        {
            return new ChallengeSelectData()
            {
                menuIndex = menuIndex,
                challengeNumber = challengeNumber,
                type = type,
            };
        }
    }

    [Net.DelayWhileTargetLoading]
    public class ChallengeSelectMessage : FusionMessageHandler
    {
        public override byte? Tag => NativeMessageTag.ChallengeSelect;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
        {
            using (FusionReader reader = FusionReader.Create(bytes))
            {
                using (var data = reader.ReadFusionSerializable<ChallengeSelectData>())
                {
                    var menu = ArenaData.GetMenu(data.menuIndex);

                    ChallengePatches.IgnorePatches = true;

                    // We ONLY handle this for clients, this message should only ever be sent by the server!
                    if (!NetworkInfo.IsServer && menu) {
                        switch (data.type) {
                            default:
                            case ChallengeSelectType.UNKNOWN:
                                break;
                            case ChallengeSelectType.SELECT_CHALLENGE:
                                menu.SelectChallenge(data.challengeNumber);
                                break;
                            case ChallengeSelectType.ON_CHALLENGE_SELECT:
                                menu.OnChallengeSelect();
                                break;
                        }
                    }

                    ChallengePatches.IgnorePatches = false;
                }
            }
        }
    }
}
