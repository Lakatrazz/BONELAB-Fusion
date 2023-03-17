using LabFusion.Extensions;
using LabFusion.Utilities;
using SLZ;

using System;

using UnityEngine;

namespace LabFusion.Data
{
    public class PDController {
        // Tweak these values to control movement properties of all synced objects
        private const float PositionFrequency = 20f;
        private const float PositionDamping = 5f;

        private const float RotationFrequency = 500f;
        private const float RotationDamping = 50f;

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
        private static float _lastFixedDelta;

        // Frame skipping logic
        // Multiplying the input deltaTime into our force equation will make up for the lost forces in the skipped frames
        private const int _targetFrame = 3;
        private const int _proportionalMult = 2;
        private readonly FrameSkipper _forceSkipper = new FrameSkipper(_targetFrame);
        private readonly FrameSkipper _torqueSkipper = new FrameSkipper(_targetFrame);

        public static void OnInitializeMelon() {
            _positionKp = CalculateKP(PositionFrequency);
            _positionKd = CalculateKD(PositionFrequency, PositionDamping);

            _rotationKp = CalculateKP(RotationFrequency);
            _rotationKd = CalculateKD(RotationFrequency, RotationDamping);
        }

        public static void OnFixedUpdate() {
            // The delta time gets multiplied in the calculations by the amount of frames we're skipping
            // This is so that the forces get scaled as if all skipped frames were still being solved
            float dt = Time.fixedDeltaTime * _targetFrame;

            // Make sure the deltaTime has changed
            if (Mathf.Approximately(dt, _lastFixedDelta)) {
                return;
            }

            _lastFixedDelta = dt;

            // Position
            float pG = 1f / (1f + _positionKd * dt + _positionKp * dt * dt);
            _positionKsg = _positionKp * pG * _proportionalMult;
            _positionKdg = (_positionKd + _positionKp * dt) * pG * _targetFrame;

            // Rotation
            float rG = 1f / (1f + _rotationKd * dt + _rotationKp * dt * dt);
            _rotationKsg = _rotationKp * rG * _targetFrame;
            _rotationKdg = (_rotationKd + _rotationKp * dt) * rG * _targetFrame;
        }

        private static float CalculateKP(float frequency) {
            return (6f * frequency) * (6f * frequency) * 0.25f;
        }

        private static float CalculateKD(float frequency, float damping) {
            return 4.5f * frequency * damping;
        }

        public Vector3 GetForce(in Rigidbody rb, in Vector3 position, in Vector3 velocity, in Vector3 targetPos, in Vector3 targetVel) {
            // Get gravity so we can apply a counter force
            Vector3 gravity = Vector3Extensions.zero;
            if (rb.useGravity)
                gravity = PhysicsUtilities.Gravity;

            // We only run forces on specific frames to save performance
            if (!_forceSkipper.IsMatchingFrame())
                return -gravity;
            
            Vector3 Pt0 = position;
            Vector3 Vt0 = velocity;

            Vector3 Pt1 = targetPos;
            Vector3 Vt1 = targetVel;

            var force = (Pt1 - Pt0) * _positionKsg + (Vt1 - Vt0) * _positionKdg - gravity;

            // Safety check
            if (force.IsNanOrInf())
                force = Vector3Extensions.zero;

            return force;
        }

        public Vector3 GetTorque(Rigidbody rb, in Quaternion rotation, in Vector3 angularVelocity, in Quaternion targetRot, in Vector3 targetVel)
        {
            // We only run torques on specific frames to save performance
            if (!_torqueSkipper.IsMatchingFrame())
                return Vector3Extensions.zero;

            Quaternion Qt1 = targetRot;
            Vector3 Vt1 = targetVel;

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

            // Safety check
            if (torque.IsNanOrInf())
                torque = Vector3Extensions.zero;

            return torque;
        }
    }
}
