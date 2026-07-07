using System.Numerics;

namespace LabFusion.Math;

/// <summary>
/// Holds the inputs and outputs of an SPD Controller so that the forces and torques may be calculated on a separate thread.
/// </summary>
public sealed class SPDState
{
    public int Count { get; }

    public Vector3[] Positions { get; }
    public Vector3[] Velocities { get; }
    public Quaternion[] Rotations { get; }
    public Vector3[] AngularVelocities { get; }

    public Vector3[] TargetPositions { get; }
    public Vector3[] TargetVelocities { get; }
    public Quaternion[] TargetRotations { get; }
    public Vector3[] TargetAngularVelocities { get; }

    public Vector3[] Forces { get; }
    public bool[] EnabledForces { get; }
    public Vector3[] Torques { get; }
    public bool[] EnabledTorques { get; }

    /// <summary>
    /// Whether or not one of the rigidbodies faced a significant desync from the target transform, requiring them to be teleported to resync.
    /// </summary>
    public bool Desynced { get; set; } = false;

    /// <summary>
    /// Whether or not forces should be calculated this frame.
    /// </summary>
    public bool CalculatingForces { get; set; } = false;

    public SPDState(int count)
    {
        Count = count;

        Positions = new Vector3[count];
        Velocities = new Vector3[count];
        Rotations = new Quaternion[count];
        AngularVelocities = new Vector3[count];

        TargetPositions = new Vector3[count];
        TargetVelocities = new Vector3[count];
        TargetRotations = new Quaternion[count];
        TargetAngularVelocities = new Vector3[count];

        Forces = new Vector3[count];
        EnabledForces = new bool[count];
        Torques = new Vector3[count];
        EnabledTorques = new bool[count];
    }

    public void SetLinearInput(int index, Vector3 position, Vector3 velocity, Vector3 targetPosition, Vector3 targetVelocity)
    {
        Positions[index] = position;
        Velocities[index] = velocity;
        TargetPositions[index] = targetPosition;
        TargetVelocities[index] = targetVelocity;
    }

    public void SetAngularInput(int index, Quaternion rotation, Vector3 angularVelocity, Quaternion targetRotation, Vector3 targetAngularVelocity)
    {
        Rotations[index] = rotation;
        AngularVelocities[index] = angularVelocity;
        TargetRotations[index] = targetRotation;
        TargetAngularVelocities[index] = targetAngularVelocity;
    }
}
