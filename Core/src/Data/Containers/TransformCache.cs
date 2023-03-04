using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.Data {
    public sealed class TransformCache {
        private Vector3 _position;
        private Quaternion _rotation;

        public Vector3 Position => _position;
        public Quaternion Rotation => _rotation;

        public void FixedUpdate(Transform transform) {
            _position = transform.position;
            _rotation = transform.rotation;
        }
    }
}
