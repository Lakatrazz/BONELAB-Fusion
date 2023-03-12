using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace LabFusion.MarrowIntegration {
#if MELONLOADER
    public class FusionMarrowBehaviour : MonoBehaviour {
#else
    public abstract class FusionMarrowBehaviour : MonoBehaviour {
#endif
#if MELONLOADER
        public FusionMarrowBehaviour(IntPtr intPtr) : base(intPtr) { }
#else
        public virtual string Comment => null;
#endif
    }
}