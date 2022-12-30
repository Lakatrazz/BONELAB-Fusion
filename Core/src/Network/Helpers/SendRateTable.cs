using LabFusion.Representation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Network {
    public static class SendRateTable {
        public static int GetPlayerSendRate() {
            var count = PlayerIdManager.PlayerCount;

            if (count <= 2)
                return 1;
            else if (count <= 5)
                return 2;
            else if (count <= 10)
                return 3;
            else if (count <= 15)
                return 4;
            else if (count <= 30)
                return 5;
            else if (count <= 50)
                return 6;
            else if (count <= 70)
                return 7;
            else if (count <= 90)
                return 8;
            else if (count <= 130)
                return 9;
            else
                return 10;
        }

        public static int GetObjectSendRate(int bytesUp) {
            if (bytesUp <= 700)
                return 1;
            else if (bytesUp <= 900)
                return 2;
            else if (bytesUp <= 1000)
                return 3;
            else if (bytesUp <= 1200)
                return 4;
            else if (bytesUp <= 1400)
                return 5;
            else if (bytesUp <= 1700)
                return 6;
            else if (bytesUp <= 2000)
                return 7;
            else if (bytesUp <= 2500)
                return 8;
            else if (bytesUp <= 2700)
                return 9;
            else
                return 10;
        }
    }
}
