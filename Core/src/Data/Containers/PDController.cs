using LabFusion.Extensions;
using LabFusion.Utilities;
using SLZ;

using System;

using UnityEngine;

namespace LabFusion.Data
{
    public class PDController {
        // Tweak these values to control movement properties of all synced objects
        private const float PositionFrequency = 10f;
        private const float PositionDamping = 3f;

        private const float RotationFrequency = 100f;
        private const float RotationDamping = 10f;

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

        // Last frame value
        private static float _lastFixedDelta = 1f;

        // Derivatives
        private bool _validPosition = false;
        private bool _validRotation = false;

        private Vector3 _lastVelocity = Vector3Extensions.zero;
        private Vector3 _lastAngularVelocity = Vector3Extensions.zero;

        private Vector3 _lastPosition = Vector3Extensions.zero;
        private Quaternion _lastRotation = QuaternionExtensions.identity;

        public static void OnInitializeMelon() {
            _positionKp = CalculateKP(PositionFrequency);
            _positionKd = CalculateKD(PositionFrequency, PositionDamping);

            _rotationKp = CalculateKP(RotationFrequency);
            _rotationKd = CalculateKD(RotationFrequency, RotationDamping);
        }

        public static void OnFixedUpdate() {
            float dt = Time.fixedDeltaTime;

            // Make sure the deltaTime has changed
            if (Mathf.Approximately(dt, _lastFixedDelta)) {
                return;
            }

            _lastFixedDelta = dt;

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

        public void ResetPosition() {
            _validPosition = false;
        }
        
        public void ResetRotation() {
            _validRotation = false;
        }

        public void Reset() {
            ResetPosition();
            ResetRotation();
        }

        public Vector3 GetForce(in Rigidbody rb, in Vector3 position, in Vector3 velocity, in Vector3 targetPos, in Vector3 targetVel) {
            // Update derivatives if needed
            if (!_validPosition) {
                _lastPosition = targetPos;
                _lastVelocity = targetVel;
                _validPosition = true;
            }
            
            // Get gravity so we can apply a counter force
            Vector3 gravity = Vector3Extensions.zero;
            if (rb.useGravity)
                gravity = PhysicsUtilities.Gravity;

            Vector3 Pt0 = position;
            Vector3 Vt0 = velocity;

            Vector3 Pt1 = _lastPosition;
            Vector3 Vt1 = _lastVelocity;

            var force = (Pt1 - Pt0) * _positionKsg + (Vt1 - Vt0) * _positionKdg - gravity;

            // Acceleration
            force += (targetVel - _lastVelocity) / _lastFixedDelta;
            _lastVelocity = targetVel;

            _lastPosition = targetPos;

            // Safety check
            if (force.IsNanOrInf())
                force = Vector3Extensions.zero;

            return force;
        }

        public Vector3 GetTorque(Rigidbody rb, in Quaternion rotation, in Vector3 angularVelocity, in Quaternion targetRot, in Vector3 targetVel)
        {
            // Update derivatives if needed
            if (!_validRotation) {
                _lastRotation = targetRot;
                _lastAngularVelocity = targetVel;
                _validRotation = true;
            }

            Quaternion Qt1 = _lastRotation;
            Vector3 Vt1 = _lastAngularVelocity;

            Quaternion q = Qt1 * Quaternion.Inverse(rotation);
            if (q.w < 0)
            {
                q.x = -q.x;
                q.y = -q.y;
                q.z = -q.z;
                q.w = -q.w;
            }
            q.ToAngleAxis(out float xMag, out Vector3 x);
            x = Vector3Extensions.Normalize(x);
            
            x *= MathExtensions.Deg2Rad;
            var torque = _rotationKsg * x * xMag + _rotationKdg * (Vt1 - angularVelocity);

            // Acceleration
            torque += (targetVel - _lastAngularVelocity) / _lastFixedDelta;
            _lastAngularVelocity = targetVel;

            _lastRotation = targetRot;

            // Safety check
            if (torque.IsNanOrInf())
                torque = Vector3Extensions.zero;

            return torque;
        }
    }
}
