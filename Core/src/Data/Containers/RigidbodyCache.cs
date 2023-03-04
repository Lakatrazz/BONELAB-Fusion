using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.Data {
    public sealed class RigidbodyCache {
        private Vector3 _velocity;
        private Vector3 _angularVelocity;
        private bool _isSleeping;

        public Vector3 Velocity => _velocity;
        public Vector3 AngularVelocity => _angularVelocity;
        public bool IsSleeping => _isSleeping;

        public void Update(Rigidbody rigidbody) {
            _isSleeping = rigidbody.IsSleeping();

            if (_isSleeping) {
                _velocity = Vector3.zero;
                _angularVelocity = Vector3.zero;
            }
            else {
                _velocity = rigidbody.velocity;
                _angularVelocity = rigidbody.angularVelocity;
            }
        }
    }
}
