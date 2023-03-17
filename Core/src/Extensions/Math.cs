using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Extensions {
    public static class MathExtensions {
        // Rad2Deg is 360 / (PI * 2) Aka 180 / PI
        public const float Rad2Deg = 57.2957795131f;

        // Deg2Rad is inverse Rad2Deg
        public const float Deg2Rad = 1f / Rad2Deg;
    }
}
