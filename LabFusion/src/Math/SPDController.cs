using LabFusion.Math.Unity;
using LabFusion.Scene;

using UnityEngine;

namespace LabFusion.Math;

public static class SPDController
{
    public const float ForceKP = 900f;
    public const float ForceKD = 200f;

    public const float TorqueKP = 1000f;
    public const float TorqueKD = 100f;

    public const float MaxForce = 1000f;
    public const float MaxTorque = 1000f;

    public static Vector3 CalculateForce(Vector3 position, Vector3 velocity, Vector3 targetPosition, Vector3 targetVelocity, float deltaTime)
    {
        if (!NetworkTransformManager.IsInBounds(targetPosition))
        {
            return Vector3.zero;
        }

        targetVelocity = NetworkTransformManager.LimitVelocity(targetVelocity);

        var nextVelocity = velocity;
        var nextPosition = position + deltaTime * nextVelocity;

        var error = targetPosition - nextPosition;
        var derivative = targetVelocity - nextVelocity;

        var g = 1f / (deltaTime * (ForceKP * deltaTime + ForceKD) + 1f);

        var kpg = ForceKP * g;
        var kdg = ForceKD * g;

        var force = kpg * error + kdg * derivative;

        force = Vector3.ClampMagnitude(force, MaxForce);

        return force;
    }

    public static Vector3 CalculateTorque(Quaternion rotation, Vector3 angularVelocity, Quaternion targetRotation, Vector3 targetAngularVelocity, float deltaTime)
    {
        var nextAngularVelocity = angularVelocity;
        var nextRotation = UnityDerivatives.GetQuaternionDisplacement(deltaTime * nextAngularVelocity) * rotation;

        var error = UnityDerivatives.GetAngularDisplacement(nextRotation, targetRotation);
        var derivative = targetAngularVelocity - nextAngularVelocity;

        var g = 1f / (deltaTime * (TorqueKP * deltaTime + TorqueKD) + 1f);

        var kpg = TorqueKP * g;
        var kdg = TorqueKD * g;

        var torque = kpg * error + kdg * derivative;

        torque = Vector3.ClampMagnitude(torque, MaxTorque);

        return torque;
    }
}