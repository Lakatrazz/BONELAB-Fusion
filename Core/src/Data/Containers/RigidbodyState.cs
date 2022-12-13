using LabFusion.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.Data {
    public class RigidbodyState {
        public float drag;
        public float angularDrag;

        public RigidbodyState(Rigidbody rb) {
            drag = rb.drag;
            angularDrag = rb.angularDrag;
        }

        public void OnClearOverride(Rigidbody rb) {
            rb.drag = drag;
            rb.angularDrag = angularDrag;
        }

        public void OnSetOverride(Rigidbody rb) {
            rb.drag = 0f;
            rb.angularDrag = 0f;
        }
    }
}
