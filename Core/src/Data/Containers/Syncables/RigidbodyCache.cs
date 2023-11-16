using LabFusion.Extensions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

using SystemVector3 = System.Numerics.Vector3;

namespace LabFusion.Data
{
    public sealed class RigidbodyCache
    {
        private SystemVector3 _velocity;
        private SystemVector3 _angularVelocity;
        private bool _isSleeping;
        private bool _isNull;

        public SystemVector3 Velocity => _velocity;
        public SystemVector3 AngularVelocity => _angularVelocity;
        public bool IsSleeping => _isSleeping;
        public bool IsNull => _isNull;

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

            if (_isSleeping)
            {
                _velocity = SystemVector3.Zero;
                _angularVelocity = SystemVector3.Zero;
            }
            else
            {
                _velocity = rigidbody.velocity.ToSystemVector3();
                _angularVelocity = rigidbody.angularVelocity.ToSystemVector3();
            }
        }
    }
}
