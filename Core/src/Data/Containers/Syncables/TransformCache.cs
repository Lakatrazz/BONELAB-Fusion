using LabFusion.Extensions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

using SystemVector3 = System.Numerics.Vector3;
using SystemQuaternion = System.Numerics.Quaternion;

namespace LabFusion.Data
{
    public sealed class TransformCache
    {
        private SystemVector3 _position;
        private SystemQuaternion _rotation;

        public SystemVector3 Position => _position;
        public SystemQuaternion Rotation => _rotation;

        public void FixedUpdate(Transform transform)
        {
            _position = transform.position.ToSystemVector3();
            _rotation = transform.rotation.ToSystemQuaternion();
        }
    }
}
