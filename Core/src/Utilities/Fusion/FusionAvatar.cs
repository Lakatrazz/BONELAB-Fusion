using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Utilities {
    public static class FusionAvatar
    {
        public const string POLY_BLANK_BARCODE = "c3534c5a-94b2-40a4-912a-24a8506f6c79";

        public static bool IsMatchingAvatar(string barcode, string target) {
            return barcode == target || barcode == POLY_BLANK_BARCODE;
        }
    }
}
