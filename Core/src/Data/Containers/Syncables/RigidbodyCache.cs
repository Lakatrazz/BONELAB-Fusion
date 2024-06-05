using LabFusion.Extensions;

using UnityEngine;

namespace LabFusion.Data
{
    public sealed class RigidbodyCache
    {
        private Vector3 _velocity;
        private Vector3 _angularVelocity;
        private bool _isSleeping;
        private bool _isNull;
        private bool _isKinematic;

        public Vector3 Velocity => _velocity;
        public Vector3 AngularVelocity => _angularVelocity;
        public bool IsSleeping => _isSleeping;
        public bool IsNull => _isNull;
        public bool IsKinematic => _isKinematic;

        public void VerifyNull(Rigidbody rigidbody)
        {
            _isNull = rigidbody == null;
        }

        public void FixedUpdate(Rigidbody rigidbody)
        {
            _isNull = rigidbody == null;

            if (_isNull)
            {
                _isSleeping = true;
                return;
            }

            _isSleeping = rigidbody.IsSleeping();

            _isKinematic = rigidbody.isKinematic;

            if (_isSleeping)
            {
                _velocity = Vector3Extensions.zero;
                _angularVelocity = Vector3Extensions.zero;
            }
            else
            {
                _velocity = rigidbody.velocity;
                _angularVelocity = rigidbody.angularVelocity;
            }
        }
    }
}
