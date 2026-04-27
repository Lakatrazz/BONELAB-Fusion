using LabFusion.Math.Numerics;
using LabFusion.Scene;

using System.Numerics;

namespace LabFusion.Math;

public static class SPDController
{
    public const float ForceKP = 900f;
    public const float ForceKD = 200f;

    public const float TorqueKP = 1000f;
    public const float TorqueKD = 100f;

    public const float MaxForce = 1000f;
    public const float MaxTorque = 1000f;

    public static float ForceG { get; private set; } = 1f;
    public static float TorqueG { get; private set; } = 1f;

    internal static void OnFixedUpdate(float deltaTime)
    {
        ForceG = CalculateG(ForceKP, ForceKD, deltaTime);
        TorqueG = CalculateG(TorqueKP, TorqueKD, deltaTime);
    }

    private static float CalculateG(float kp, float kd, float deltaTime)
    {
        return 1f / (deltaTime * (kp * deltaTime + kd) + 1f);
    }

    public static Vector3 CalculateForce(Vector3 position, Vector3 velocity, Vector3 targetPosition, Vector3 targetVelocity, float deltaTime)
    {
        if (!NetworkTransformManager.IsInBounds(targetPosition))
        {
            return Vector3.Zero;
        }

        targetVelocity = NetworkTransformManager.LimitVelocity(targetVelocity);

        var nextVelocity = velocity;
        var nextPosition = position + deltaTime * nextVelocity;

        var error = targetPosition - nextPosition;
        var derivative = targetVelocity - nextVelocity;

        var kpg = ForceKP * ForceG;
        var kdg = ForceKD * ForceG;

        var force = kpg * error + kdg * derivative;

        force = NumericsMathVector3.ClampMagnitude(force, MaxForce);

        return force;
    }

    public static Vector3 CalculateTorque(Quaternion rotation, Vector3 angularVelocity, Quaternion targetRotation, Vector3 targetAngularVelocity, float deltaTime)
    {
        var nextAngularVelocity = angularVelocity;
        var nextRotation = NumericsDerivatives.GetQuaternionDisplacement(deltaTime * nextAngularVelocity) * rotation;

        var error = NumericsDerivatives.GetAngularDisplacement(nextRotation, targetRotation);
        var derivative = targetAngularVelocity - nextAngularVelocity;

        var kpg = TorqueKP * TorqueG;
        var kdg = TorqueKD * TorqueG;

        var torque = kpg * error + kdg * derivative;

        torque = NumericsMathVector3.ClampMagnitude(torque, MaxTorque);

        return torque;
    }
}