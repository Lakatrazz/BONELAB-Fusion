using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.SDK.Points {
    public static class BitEconomy {
        public const double BitsToPennyRatio = 1;

        public const string BaBaAuthor = "BaBaCorp";

        public static int ConvertPrice(int pennies) {
            return (int)(BitsToPennyRatio * (double)pennies);
        }
    }
}
