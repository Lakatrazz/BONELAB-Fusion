using LabFusion.Representation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Network {
    public static class SendRateTable {
        private static readonly int[] _playerTable = new int[9] {
            2,
            5,
            10,
            15,
            30,
            50,
            70,
            90,
            130,
        };

        private static readonly int[] _objectTable = new int[14] {
            700,
            900,
            1000,
            1200,
            1400,
            1700,
            2000,
            2500,
            2700,
            3000,
            3200,
            3400,
            3700,
            4200,
        };

        public static int GetPlayerSendRate() {
            var count = PlayerIdManager.PlayerCount;
            var tableLength = _playerTable.Length;

            for (var i = 0; i < tableLength; i++) {
                if (count <= _playerTable[i])
                    return i + 1;
            }

            return tableLength + 1;
        }

        public static int GetObjectSendRate(int bytesUp)
        {
            var tableLength = _objectTable.Length;

            for (var i = 0; i < tableLength; i++) {
                if (bytesUp <= _objectTable[i])
                    return i + 1;
            }

            return tableLength + 1;
        }

    }
}
