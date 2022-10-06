using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MelonLoader;

using UnhollowerRuntimeLib;

using UnityEngine;

namespace LabFusion
{
    public class FusionMod : MelonMod
    {
        public struct FusionVersion {
            public const byte versionMajor = 0;
            public const byte versionMinor = 0;
            public const short versionPatch = 1;
        }

        public static readonly Version Version = new Version(FusionVersion.versionMajor, FusionVersion.versionMinor, FusionVersion.versionPatch);
    }
}
