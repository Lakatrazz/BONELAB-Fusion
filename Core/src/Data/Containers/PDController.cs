using LabFusion.Extensions;

using SLZ;

using System;

using UnityEngine;

namespace LabFusion.Data
{
    public class PDController {
        // Constant to replace stripped(?) value
        public const float Deg2Rad = (float)Math.PI * 2f / 360f;

        // Tweak these values to control movement properties of all synced objects
        public const float frequency = 20f;
        public const float damping = 3f;

        private readonly float _kp;
        private readonly float _kd;

        public Vector3 LastTargetPos { get; private set; }

        public Quaternion LastTargetRot { get; private set; }

        public PDController() {
            _kp = (6f * frequency) * (6f * frequency) * 0.25f;
            _kd = 4.5f * frequency * damping;
        }

        public void OnResetDerivatives(in Rigidbody rb) {
            OnResetPosDerivatives(rb);
            OnResetRotDerivatives(rb);
        }

        public void OnResetPosDerivatives(in Rigidbody rb)
        {
            LastTargetPos = rb.transform.position;
        }

        public void OnResetRotDerivatives(in Rigidbody rb)
        {
            LastTargetRot = rb.transform.rotation;
        }

        public Vector3 GetForce(in Rigidbody rb, in Vector3 targetPos) {
            CalculateFrameValues(out float ksg, out float kdg);

            Vector3 Pt0 = rb.transform.position;
            Vector3 Vt0 = rb.velocity;

            Vector3 Pt1 = LastTargetPos;
            Vector3 Vt1 = PhysXUtils.GetLinearVelocity(LastTargetPos, targetPos);

            var force = (Pt1 - Pt0) * ksg + (Vt1 - Vt0) * kdg - (rb.useGravity ? Physics.gravity : Vector3.zero);

            LastTargetPos = targetPos;

            // Safety check
            if (force.IsNanOrInf())
                force = Vector3.zero;

            return force;
        }

        public Vector3 GetTorque(Rigidbody rb, in Quaternion targetRot)
        {
            CalculateFrameValues(out float ksg, out float kdg);

            var currentRotation = rb.transform.rotation;

            Quaternion Qt1 = LastTargetRot;
            Vector3 Vt1 = PhysXUtils.GetAngularVelocity(LastTargetRot, targetRot);

            Quaternion q = Qt1 * Quaternion.Inverse(currentRotation);
            if (q.w < 0)
            {
                q.x = -q.x;
                q.y = -q.y;
                q.z = -q.z;
                q.w = -q.w;
            }
            q.ToAngleAxis(out float xMag, out Vector3 x);
            x.Normalize();
            
            x *= Deg2Rad;
            var torque = ksg * x * xMag + kdg * (Vt1 - rb.angularVelocity);

            LastTargetRot = targetRot;

            // Safety check
            if (torque.IsNanOrInf())
                torque = Vector3.zero;

            return torque;
        }

        private void CalculateFrameValues(out float ksg, out float kdg) {
            float dt = Time.fixedDeltaTime;
            float g = 1 / (1 + _kd * dt + _kp * dt * dt);
            ksg = _kp * g;
            kdg = (_kd + _kp * dt) * g;
        }
    }
}
