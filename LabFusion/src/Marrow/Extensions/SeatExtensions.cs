using Il2CppSLZ.Marrow;

namespace LabFusion.Marrow.Extensions;

public static class SeatExtensions
{
    /// <summary>
    /// Teleports the seated rig to the seat position.
    /// </summary>
    /// <param name="seat"></param>
    public static void TeleportRigToSeat(this Seat seat)
    {
        var rigManager = seat.rigManager;

        if (rigManager == null)
        {
            return;
        }

        var buttJoint = seat.buttJoint;
        var buttPosition = buttJoint.transform.TransformPoint(buttJoint.swapBodies ? buttJoint.connectedAnchor : buttJoint.anchor);
        var buttVelocity = rigManager.physicsRig.torso.rbPelvis.velocity;

        var buttTargetPosition = seat.buttTargetInWorld;
        var buttTargetVelocity = seat.seatRb.velocity;

        var positionOffset = buttTargetPosition - buttPosition;
        var velocityOffset = buttTargetVelocity - buttVelocity;

        rigManager.TeleportWithOffset(positionOffset, velocityOffset);
    }
}
