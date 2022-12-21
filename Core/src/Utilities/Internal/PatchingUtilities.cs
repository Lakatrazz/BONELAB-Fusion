using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine.SceneManagement;
using UnityEngine.Events;
using BoneLib;

using LabFusion.Patching;

namespace LabFusion.Utilities {
    internal static class PatchingUtilities {
        internal static void PatchAll() {
            VirtualControllerPatches.Patch();
        }
    }
}
