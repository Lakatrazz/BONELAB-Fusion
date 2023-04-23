using LabFusion.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Representation {
    public enum FusionMasterResult {
        NORMAL = 1 << 0,
        IMPERSONATOR = 1 << 1,
        MASTER = 1 << 2,
    }

    public static class FusionMasterList {
        public struct MasterPlayer {
            public ulong id;
            public string name;

            public MasterPlayer(ulong id, string name) {
                this.id = id;
                this.name = name;
            }
        }

        private static readonly MasterPlayer[] _steamPlayers = new MasterPlayer[] {
            new MasterPlayer(76561198198752494, "Lakatrazz"),
            new MasterPlayer(76561198097630377, "AlexTheBaBa"),
        };

        public static FusionMasterResult VerifyPlayer(ulong id, string name) {
            if (NetworkInfo.CurrentNetworkLayer is SteamNetworkLayer)
                return Internal_VerifyPlayer(_steamPlayers, id, name);
            
            return FusionMasterResult.NORMAL;
        }

        private static FusionMasterResult Internal_VerifyPlayer(MasterPlayer[] players, ulong id, string name) {
            for (var i = 0; i < players.Length; i++) {
                var player = players[i];

                // Our id matches, and this is a master user
                if (player.id == id) {
                    return FusionMasterResult.MASTER;
                }

                // The name matches, but the id didn't
                if (player.name == name) {
                    return FusionMasterResult.IMPERSONATOR;
                }
            }

            // Neither name nor id matched, this is a regular joe
            return FusionMasterResult.NORMAL;
        }
    }
}
