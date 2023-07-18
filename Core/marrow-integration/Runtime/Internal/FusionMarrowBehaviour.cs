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

        private Transform _transform;
        private bool _hasTransform = false;
        public Transform Transform {
            get {
                if (!_hasTransform) {
                    _transform = transform;
                    _hasTransform = true;
                }

                return _transform;
            }
        }
#else
        public virtual string Comment => null;
#endif
    }
}