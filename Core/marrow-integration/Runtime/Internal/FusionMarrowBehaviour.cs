using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace LabFusion.MarrowIntegration {
    public abstract class FusionMarrowBehaviour : MonoBehaviour {
#if MELONLOADER
        public FusionMarrowBehaviour(IntPtr intPtr) : base(intPtr) { }
#else
        public virtual string Comment => null;
#endif
    }
}