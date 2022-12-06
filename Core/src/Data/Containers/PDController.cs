using PathCreation.Utility;
using SLZ;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.Data
{
    public class PDController {
        // Constant to replace stripped(?) value
        public const float Deg2Rad = ((float)Math.PI * 2f) / 360f;

        // Tweak these values to control movement properties of all synced objects
        public const float frequency = 20f;
        public const float damping = 3f;

        private readonly float _kp;
        private readonly float _kd;

        public Vector3 LastTargetPos { get; private set; }
        private Vector3 _lastInstantVel;

        public Quaternion LastTargetRot { get; private set; }
        private Vector3 _lastInstantAngVel;

        public PDController() {
            _kp = (6f * frequency) * (6f * frequency) * 0.25f;
            _kd = 4.5f * frequency * damping;
        }

        public Vector3 GetInstantAccelerationForce(in Rigidbody rb, in Vector3 targetPos) {
            var vel = PhysXUtils.GetLinearVelocity(LastTargetPos, targetPos);
            var accel = PhysXUtils.GetLinearVelocity(_lastInstantVel, vel);

            _lastInstantVel = vel;
            LastTargetPos = targetPos;

            return accel;
        }

        public Vector3 GetInstantAccelerationTorque(in Rigidbody rb, in Quaternion targetRot)
        {
            var angVel = PhysXUtils.GetAngularVelocity(LastTargetRot, targetRot);
            var accel = PhysXUtils.GetLinearVelocity(_lastInstantAngVel, angVel);

            _lastInstantAngVel = angVel;
            LastTargetRot = targetRot;

            return accel;
        }

        public void OnResetDerivatives(in Rigidbody rb) {
            _lastInstantAngVel = Vector3.zero;
            _lastInstantVel = Vector3.zero;

            LastTargetPos = rb.transform.position;
            LastTargetRot = rb.transform.rotation;
        }

        public void OnResetPosDerivatives(in Rigidbody rb)
        {
            _lastInstantVel = Vector3.zero;
            LastTargetPos = rb.transform.position;
        }

        public void OnResetRotDerivatives(in Rigidbody rb)
        {
            _lastInstantAngVel = Vector3.zero;
            LastTargetRot = rb.transform.rotation;
        }

        public Vector3 GetForce(in Rigidbody rb, in Vector3 targetPos) {
            CalculateFrameValues(out float ksg, out float kdg);

            Vector3 Pt0 = rb.transform.position;
            Vector3 Vt0 = rb.velocity;
            return ((targetPos - Pt0) * ksg + (-Vt0) * kdg) - (rb.useGravity ? Physics.gravity : Vector3.zero);
        }

        public Vector3 GetTorque(Rigidbody rb, in Quaternion targetRot)
        {
            CalculateFrameValues(out float ksg, out float kdg);

            var currentRotation = rb.transform.rotation;

            Quaternion q = targetRot * Quaternion.Inverse(currentRotation);
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
            return ksg * x * xMag - kdg * rb.angularVelocity;
        }

        private void CalculateFrameValues(out float ksg, out float kdg) {
            float dt = Time.fixedDeltaTime;
            float g = 1 / (1 + _kd * dt + _kp * dt * dt);
            ksg = _kp * g;
            kdg = (_kd + _kp * dt) * g;
        }
    }
}
