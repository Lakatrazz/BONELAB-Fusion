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
        private const float PositionFrequency = 20f;
        private const float PositionDamping = 3f;

        private const float RotationFrequency = 500f;
        private const float RotationDamping = 100f;

        // Calculated KP and KD values for adding forces. These are only calculated once
        private static float _positionKp;
        private static float _positionKd;

        private static float _rotationKp;
        private static float _rotationKd;

        // Calculated KSG and KDG values to multiply the forces, these are calculated once per frame
        private static float _positionKsg;
        private static float _positionKdg;

        private static float _rotationKsg;
        private static float _rotationKdg;

        public Vector3 LastTargetPos { get; private set; }

        public Quaternion LastTargetRot { get; private set; }

        public static void OnMelonInitialize() {
            _positionKp = CalculateKP(PositionFrequency);
            _positionKd = CalculateKD(PositionFrequency, PositionDamping);

            _rotationKp = CalculateKP(RotationFrequency);
            _rotationKd = CalculateKD(RotationFrequency, RotationDamping);
        }

        public static void OnFixedUpdate() {
            float dt = Time.fixedDeltaTime;

            // Position
            float pG = 1f / (1f + _positionKd * dt + _positionKp * dt * dt);
            _positionKsg = _positionKp * pG;
            _positionKdg = (_positionKd + _positionKp * dt) * pG;

            // Rotation
            float rG = 1f / (1f + _rotationKd * dt + _rotationKp * dt * dt);
            _rotationKsg = _rotationKp * rG;
            _rotationKdg = (_rotationKd + _rotationKp * dt) * rG;
        }

        private static float CalculateKP(float frequency) {
            return (6f * frequency) * (6f * frequency) * 0.25f;
        }

        private static float CalculateKD(float frequency, float damping) {
            return 4.5f * frequency * damping;
        }

        public void OnResetDerivatives(in Rigidbody rb) {
            OnResetPosDerivatives(rb);
            OnResetRotDerivatives(rb);
        }

        public void OnResetPosDerivatives(in Rigidbody rb) {
            LastTargetPos = rb.transform.position;
        }

        public void OnResetRotDerivatives(in Rigidbody rb) {
            LastTargetRot = rb.transform.rotation;
        }

        public Vector3 GetForce(in Rigidbody rb, in Vector3 targetPos) {
            Vector3 Pt0 = rb.transform.position;
            Vector3 Vt0 = rb.velocity;

            Vector3 Pt1 = LastTargetPos;
            Vector3 Vt1 = PhysXUtils.GetLinearVelocity(LastTargetPos, targetPos);

            var force = (Pt1 - Pt0) * _positionKsg + (Vt1 - Vt0) * _positionKdg - (rb.useGravity ? Physics.gravity : Vector3.zero);

            LastTargetPos = targetPos;

            // Safety check
            if (force.IsNanOrInf())
                force = Vector3.zero;

            return force;
        }

        public Vector3 GetTorque(Rigidbody rb, in Quaternion targetRot)
        {
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
            var torque = _rotationKsg * x * xMag + _rotationKdg * (Vt1 - rb.angularVelocity);

            LastTargetRot = targetRot;

            // Safety check
            if (torque.IsNanOrInf())
                torque = Vector3.zero;

            return torque;
        }
    }
}
