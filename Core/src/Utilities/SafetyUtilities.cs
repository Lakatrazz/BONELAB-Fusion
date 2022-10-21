using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.Utilities {
    public static class SafetyUtilities {
        public static bool IsValidTime => Time.timeScale > 0f && Time.deltaTime > 0f && Time.fixedDeltaTime > 0f;
    }
}
