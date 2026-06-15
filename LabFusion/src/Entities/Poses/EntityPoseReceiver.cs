using LabFusion.Math;
using LabFusion.Network;

namespace LabFusion.Entities;

public class EntityPoseReceiver
{
    /// <summary>
    /// The number of bodies in each pose.
    /// </summary>
    public int BodyCount { get; private set; } = 0;

    /// <summary>
    /// The pose in use right before the latest pose was received from the entity's owner.
    /// Used for interpolation to smooth out network latency.
    /// <para>Only valid if <see cref="HasReceivedPose"/> is true.</para>
    /// </summary>
    public EntityPose LastReceivedPose { get; private set; } = null;

    /// <summary>
    /// The latest pose received from the entity's's owner.
    /// This is the pose that prediction is based on, and will be used to replicate the entity's locally if the owner stops sending messages.
    /// <para>Only valid if <see cref="HasReceivedPose"/> is true.</para>
    /// </summary>
    public EntityPose ReceivedPose { get; private set; } = null;

    /// <summary>
    /// The <see cref="ReceivedPose"/>, but predicted right after being received based on the network latency.
    /// <para>Only valid if <see cref="HasReceivedPose"/> is true.</para>
    /// </summary>
    public EntityPose PredictedPose { get; private set; } = null;

    /// <summary>
    /// The current interpolated pose between <see cref="LastReceivedPose"/> and <see cref="PredictedPose"/>.
    /// Additional prediction based on velocity is applied on top to simulate the object's travel while waiting for the next pose.
    /// This is the pose actively used to replicate entities locally per physics update.
    /// <para>Only valid if <see cref="HasReceivedPose"/> is true.</para>
    /// </summary>
    public EntityPose InterpolatedPose { get; private set; } = null;

    /// <summary>
    /// Whether an updated pose has been received from the entity's owner and applied to <see cref="ReceivedPose"/>.
    /// </summary>
    public bool HasReceivedPose { get; private set; } = false;

    /// <summary>
    /// The time in seconds since we received an EntityPose from the entity's owner.
    /// </summary>
    public float TimeSinceReceivedPose { get; private set; } = 0f;

    /// <summary>
    /// The percent from 0 to 1 of interpolation from <see cref="LastReceivedPose"/> to <see cref="PredictedPose"/>.
    /// </summary>
    public float InterpolationPercent { get; private set; } = 0f;

    /// <summary>
    /// Initializes all the poses with a certain amount of bodies.
    /// </summary>
    /// <param name="bodyCount"></param>
    public void InitializePoses(int bodyCount)
    {
        BodyCount = bodyCount;

        LastReceivedPose = new(bodyCount);
        ReceivedPose = new(bodyCount);
        PredictedPose = new(bodyCount);
        InterpolatedPose = new(bodyCount);
    }

    /// <summary>
    /// Receives a pose and begins interpolation and prediction.
    /// </summary>
    /// <param name="pose"></param>
    public void ReceivePose(EntityPose pose)
    {
        if (HasReceivedPose)
        {
            InterpolatedPose.WriteTo(LastReceivedPose);
        }
        else
        {
            pose.WriteTo(LastReceivedPose);
        }

        pose.WriteTo(ReceivedPose);

        pose.WriteTo(PredictedPose);

        float predictionTime = MathF.Min(TimeSinceReceivedPose, NetworkTickManager.MaxPredictionTime);

        PredictedPose.Predict(predictionTime);

        LastReceivedPose.WriteTo(InterpolatedPose);

        RefreshPoseState();
    }

    /// <summary>
    /// Ticks the time since a pose has been received and resolves the interpolation and prediction of the output pose.
    /// </summary>
    /// <param name="deltaTime"></param>
    public void TickPose(float deltaTime) => TickPose(deltaTime, true);

    /// <summary>
    /// Ticks the time since a pose has been received. If resolvePose is true, the output pose will be updated with interpolation and prediction.
    /// </summary>
    /// <param name="deltaTime"></param>
    /// <param name="resolvePose"></param>
    public void TickPose(float deltaTime, bool resolvePose)
    {
        // The time since a pose has been received should still increment regardless if the first pose has been received
        // This is so that initial prediction still works during the time when ownership has changed and a new pose hasn't been received yet
        TimeSinceReceivedPose += deltaTime;

        if (!resolvePose)
        {
            return;
        }

        if (!HasReceivedPose)
        {
            return;
        }

        InterpolationPercent = ManagedMathf.Clamp01(TimeSinceReceivedPose / NetworkTickManager.LinearInterpolationLength);

        InterpolatedPose.Interpolate(LastReceivedPose, PredictedPose, InterpolationPercent);

        float predictionTime = MathF.Min(TimeSinceReceivedPose, NetworkTickManager.MaxPredictionTime);

        InterpolatedPose.PredictFrom(predictionTime, ReceivedPose);
    }

    /// <summary>
    /// Writes the received pose to the other poses to prevent prediction or interpolation logic.
    /// </summary>
    public void ResyncReceivedPose()
    {
        ReceivedPose.WriteTo(LastReceivedPose);
        ReceivedPose.WriteTo(PredictedPose);
        ReceivedPose.WriteTo(InterpolatedPose);
    }

    /// <summary>
    /// Sets HasReceivedPose to true and clears the time since a pose was received.
    /// This should be called when a new pose is received or the client wants to act as if one has been received.
    /// </summary>
    public void RefreshPoseState()
    {
        TimeSinceReceivedPose = 0f;
        HasReceivedPose = true;
    }

    /// <summary>
    /// Sets HasReceivedPose to false and clears the time since a pose was received.
    /// This should be called when the client should no longer treat a pose as having been received.
    /// </summary>
    public void ClearPoseState()
    {
        TimeSinceReceivedPose = 0f;
        HasReceivedPose = false;
    }
}
