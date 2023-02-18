using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.SDK.Points {
    public static class BitEconomy {
        public const double BitsToPennyRatio = 0;
        public const int DefaultMaxBitsPerRound = (int)(100d * BitsToPennyRatio);

        public const string InternalAuthor = "BONELAB Fusion";
        public const string BaBaAuthor = "BaBaCorp";

        public static int GetCost(int pennies) {
            return (int)(BitsToPennyRatio * (double)pennies);
        }
    }
}
