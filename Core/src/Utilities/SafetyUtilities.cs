using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.Utilities
{
    public static class SafetyUtilities
    {
        public static bool IsValidTime => TimeUtilities.TimeScale > 0f && TimeUtilities.DeltaTime > 0f && TimeUtilities.FixedDeltaTime > 0f;
    }
}
