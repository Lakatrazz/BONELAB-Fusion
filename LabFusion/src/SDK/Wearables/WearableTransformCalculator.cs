using Il2CppSLZ.Marrow;

using LabFusion.Extensions;
using LabFusion.Marrow.Integration;

using UnityEngine;

using Avatar = Il2CppSLZ.VRMK.Avatar;

namespace LabFusion.SDK.Wearables;

public static class WearableTransformCalculator
{
    public static void GetTransform(AvatarPointOverride point, out Vector3 position, out Quaternion rotation, out Vector3 scale)
    {
        var transform = point.transform;
        position = transform.position;
        rotation = transform.rotation;
        scale = transform.lossyScale;
    }

    public static void GetTransform(AvatarAnchor anchor, RigManager rigManager, out Vector3 position, out Quaternion rotation, out Vector3 scale)
    {
        PhysicsRig physicsRig = rigManager.physicsRig;
        ArtRig artRig = physicsRig.artOutput;
        Avatar avatar = rigManager.avatar;

        scale = GetScale(avatar, anchor.Point);

        switch (anchor.Point)
        {
            default:
            case AvatarPoint.Head:
                GetHeadTransform(anchor.Alignment, physicsRig, artRig, avatar, out position, out rotation);
                break;
            case AvatarPoint.HeadTop:
                GetHeadTopTransform(physicsRig, artRig, avatar, out position, out rotation);
                break;
            case AvatarPoint.Eye:
                GetEyeTransform(anchor.Side, physicsRig, artRig, out position, out rotation);
                break;
            case AvatarPoint.Chest:
                GetChestTransform(anchor.Alignment, physicsRig, artRig, avatar, out position, out rotation);
                break;
            case AvatarPoint.Hips:
                GetHipsTransform(anchor.Alignment, physicsRig, artRig, avatar, out position, out rotation);
                break;
            case AvatarPoint.Wrist:
                GetWristTransform(anchor.Alignment, anchor.Side, artRig, avatar, out position, out rotation);
                break;
            case AvatarPoint.Ankle:
                GetAnkleTransform(anchor.Alignment, anchor.Side, physicsRig, artRig, avatar, out position, out rotation);
                break;
        }
    }

    public static Vector3 GetScale(Avatar avatar, AvatarPoint point)
    {
        var scale = Vector3Extensions.One;

        float eyeHeightProportion = avatar.eyeHeight / WearableConstants.ReferenceEyeHeight;

        switch (point)
        {
            default:
                scale *= eyeHeightProportion;
                break;
            case AvatarPoint.Head:
            case AvatarPoint.HeadTop:
            case AvatarPoint.Eye:
                {
                    float radiusX = avatar.ForeheadEllipseX;
                    float radiusZ = (avatar.ForeheadEllipseZ + avatar.ForeheadEllipseNegZ) * 0.5f;

                    float referenceRadiusX = WearableConstants.ReferenceForeheadEllipseX;
                    float referenceRadiusZ = (WearableConstants.ReferenceForeheadEllipseZ + WearableConstants.ReferenceForeheadEllipseNegZ) * 0.5f;

                    scale = GetScale(radiusX, radiusZ, referenceRadiusX, referenceRadiusZ) * eyeHeightProportion;
                }
                break;
            case AvatarPoint.Chest:
                {
                    float radiusX = avatar.ChestEllipseX;
                    float radiusZ = (avatar.ChestEllipseZ + avatar.ChestEllipseNegZ) * 0.5f;

                    float referenceRadiusX = WearableConstants.ReferenceChestEllipseX;
                    float referenceRadiusZ = (WearableConstants.ReferenceChestEllipseZ + WearableConstants.ReferenceChestEllipseNegZ) * 0.5f;

                    scale = GetScale(radiusX, radiusZ, referenceRadiusX, referenceRadiusZ) * eyeHeightProportion;
                }
                break;
            case AvatarPoint.Hips:
                {
                    float radiusX = avatar.HipsEllipseX;
                    float radiusZ = (avatar.HipsEllipseZ + avatar.HipsEllipseNegZ) * 0.5f;

                    float referenceRadiusX = WearableConstants.ReferenceHipsEllipseX;
                    float referenceRadiusZ = (WearableConstants.ReferenceHipsEllipseZ + WearableConstants.ReferenceHipsEllipseNegZ) * 0.5f;

                    scale = GetScale(radiusX, radiusZ, referenceRadiusX, referenceRadiusZ) * eyeHeightProportion;
                }
                break;
            case AvatarPoint.Wrist:
                {
                    float radiusX = avatar.wristEllipse.XRadius;
                    float radiusZ = avatar.wristEllipse.ZRadius;

                    float referenceRadiusX = WearableConstants.ReferenceWristEllipseX;
                    float referenceRadiusZ = WearableConstants.ReferenceWristEllipseZ;

                    scale = GetScale(radiusX, radiusZ, referenceRadiusX, referenceRadiusZ) * eyeHeightProportion;
                }
                break;
            case AvatarPoint.Ankle:
                {
                    float radiusX = avatar.ankleEllipse.XRadius;
                    float radiusZ = avatar.ankleEllipse.ZRadius;

                    float referenceRadiusX = WearableConstants.ReferenceAnkleEllipseX;
                    float referenceRadiusZ = WearableConstants.ReferenceAnkleEllipseZ;

                    scale = GetScale(radiusX, radiusZ, referenceRadiusX, referenceRadiusZ) * eyeHeightProportion;
                }
                break;
        }

        return scale;
    }

    private static Vector3 GetScale(float radiusX, float radiusZ, float referenceRadiusX, float referenceRadiusZ)
    {
        float radiusMax = MathF.Max(radiusX, radiusZ);
        float referenceRadiusMax = MathF.Max(referenceRadiusX, referenceRadiusZ);

        float radiusProportion = radiusMax / referenceRadiusMax;

        return Vector3Extensions.One * radiusProportion;
    }

    private static void GetHeadTransform(AvatarAlignment alignment, PhysicsRig physicsRig, ArtRig artRig, Avatar avatar, out Vector3 position, out Quaternion rotation)
    {
        var head = physicsRig.m_head;

        float eyeHeight = avatar.eyeHeight;
        var eyeCenter = GetEyeCenter(artRig);

        var headForward = head.forward;
        var headUp = head.up;
        var headRight = head.right;

        var headPosition = head.position;
        var headRotation = head.rotation;

        var headCenterY = GetPositionWithRelativeY(head, headPosition, eyeCenter, 0.5f * eyeHeight * (avatar.HeadTop - avatar.ChinY));
        var headCenterPosition = headCenterY + eyeHeight * GetHeadZOffset(avatar) * headForward;

        var frontOffsetRotation = Quaternion.AngleAxis(90f, headRight) * headRotation;

        switch (alignment)
        {
            default:
            case AvatarAlignment.Center:
                position = headCenterPosition;
                rotation = headRotation;
                break;
            case AvatarAlignment.Front:
                position = headCenterY + eyeHeight * avatar.ForeheadEllipseZ * headForward;
                rotation = frontOffsetRotation;
                break;
            case AvatarAlignment.Back:
                position = headCenterY - eyeHeight * avatar.ForeheadEllipseNegZ * headForward;
                rotation = Quaternion.AngleAxis(180f, headUp) * frontOffsetRotation;
                break;
            case AvatarAlignment.Out:
                position = headCenterPosition + eyeHeight * avatar.ForeheadEllipseX * headRight;
                rotation = Quaternion.AngleAxis(90f, headUp) * frontOffsetRotation;
                break;
            case AvatarAlignment.In:
                position = headCenterPosition - eyeHeight * avatar.ForeheadEllipseX * headRight;
                rotation = Quaternion.AngleAxis(-90f, headUp) * frontOffsetRotation;
                break;
        }
    }

    private static void GetHeadTopTransform(PhysicsRig physicsRig, ArtRig artRig, Avatar avatar, out Vector3 position, out Quaternion rotation)
    {
        var head = physicsRig.m_head;

        var eyeCenter = GetEyeCenter(artRig);
        var eyeHeight = avatar.eyeHeight;

        var headCenterXZ = head.position + eyeHeight * GetHeadZOffset(avatar) * head.forward;

        var headTop = GetPositionWithRelativeY(head, headCenterXZ, eyeCenter, eyeHeight * avatar.HeadTop);

        position = headTop;
        rotation = head.rotation;
    }

    private static void GetEyeTransform(AvatarSide side, PhysicsRig physicsRig, ArtRig artRig, out Vector3 position, out Quaternion rotation)
    {
        var head = physicsRig.m_head;

        position = side switch
        {
            AvatarSide.Left => artRig.eyeLf.position,
            AvatarSide.Right => artRig.eyeRt.position,
            _ => GetEyeCenter(artRig),
        };

        rotation = head.rotation;
    }

    private static void GetChestTransform(AvatarAlignment alignment, PhysicsRig physicsRig, ArtRig artRig, Avatar avatar, out Vector3 position, out Quaternion rotation)
    {
        var t7Vert = artRig.t7Vert;

        float eyeHeight = avatar.eyeHeight;

        var chestForward = t7Vert.forward;
        var chestUp = t7Vert.up;
        var chestRight = t7Vert.right;

        var chestCenterPosition = t7Vert.position - eyeHeight * avatar.t7OffsetZ * chestForward;
        var chestCenterRotation = t7Vert.rotation;

        var frontOffsetRotation = Quaternion.AngleAxis(90f, chestRight) * chestCenterRotation;

        switch (alignment)
        {
            default:
            case AvatarAlignment.Center:
                position = chestCenterPosition;
                rotation = chestCenterRotation;
                break;
            case AvatarAlignment.Back:
                position = chestCenterPosition - avatar.ChestEllipseNegZ * eyeHeight * chestForward;
                rotation = Quaternion.AngleAxis(180f, chestUp) * frontOffsetRotation;
                break;
            case AvatarAlignment.Front:
                position = chestCenterPosition + avatar.ChestEllipseZ * eyeHeight * chestForward;
                rotation = frontOffsetRotation;
                break;
            case AvatarAlignment.Out:
                position = chestCenterPosition + avatar.ChestEllipseX * eyeHeight * chestRight;
                rotation = Quaternion.AngleAxis(90f, chestUp) * frontOffsetRotation;
                break;
            case AvatarAlignment.In:
                position = chestCenterPosition - avatar.ChestEllipseX * eyeHeight * chestRight;
                rotation = Quaternion.AngleAxis(-90f, chestUp) * frontOffsetRotation;
                break;
        }
    }

    private static void GetHipsTransform(AvatarAlignment alignment, PhysicsRig physicsRig, ArtRig artRig, Avatar avatar, out Vector3 position, out Quaternion rotation)
    {
        var pelvis = physicsRig.m_pelvis;

        float eyeHeight = avatar.eyeHeight;

        var hipsForward = pelvis.forward;
        var hipsUp = pelvis.up;
        var hipsRight = pelvis.right;

        var hipsPosition = pelvis.position;

        var hipsCenterPosition = hipsPosition + 0.5f * eyeHeight * (avatar.HipsEllipseZ - avatar.HipsEllipseNegZ) * hipsForward;
        var hipsCenterRotation = pelvis.rotation;

        var frontOffsetRotation = Quaternion.AngleAxis(90f, hipsRight) * hipsCenterRotation;


        switch (alignment)
        {
            default:
            case AvatarAlignment.Center:
                position = hipsCenterPosition;
                rotation = hipsCenterRotation;
                break;
            case AvatarAlignment.Back:
                position = hipsPosition - avatar.HipsEllipseNegZ * eyeHeight * hipsForward;
                rotation = Quaternion.AngleAxis(180f, hipsUp) * frontOffsetRotation;
                break;
            case AvatarAlignment.Front:
                position = hipsPosition + avatar.HipsEllipseZ * eyeHeight * hipsForward;
                rotation = frontOffsetRotation;
                break;
            case AvatarAlignment.Out:
                position = hipsCenterPosition + avatar.HipsEllipseX * eyeHeight * hipsRight;
                rotation = Quaternion.AngleAxis(90f, hipsUp) * frontOffsetRotation;
                break;
            case AvatarAlignment.In:
                position = hipsCenterPosition - avatar.HipsEllipseX * eyeHeight * hipsRight;
                rotation = Quaternion.AngleAxis(-90f, hipsUp) * frontOffsetRotation;
                break;
        }
    }

    private static void GetWristTransform(AvatarAlignment alignment, AvatarSide side, ArtRig artRig, Avatar avatar, out Vector3 position, out Quaternion rotation)
    {
        bool left = side != AvatarSide.Right;

        float eyeHeight = avatar.eyeHeight;
        float wristSideRadius = avatar.wristEllipse.ZRadius * eyeHeight;
        float wristUpRadius = avatar.wristEllipse.XRadius * eyeHeight;

        Transform wristTransform = left ? artRig.wristLf : artRig.wristRt;
        var wristPosition = wristTransform.position;
        var wristRotation = wristTransform.rotation;

        // In is the direction towards the body, or the direction of the thumb
        var inAxis = wristTransform.up;

        // Up is the direction out from the back of the hand
        var upAxis = (left ? -1f : 1f) * wristTransform.right;

        // Direction of the fingers
        var fingerAxis = wristTransform.forward;

        // Slight tilt to align the rotation more with the wrist
        float tiltAngle = 8f;

        if (left)
        {
            tiltAngle *= -1f;
        }

        switch (alignment)
        {
            case AvatarAlignment.In:
            case AvatarAlignment.Out:
                tiltAngle = 0f;
                break;
            case AvatarAlignment.Back:
                tiltAngle *= -1f;
                break;
        }

        var tiltOffset = Quaternion.AngleAxis(tiltAngle, inAxis);

        // Offset to correct the given rotation so that forward is the direction of the fingers and up is out from the back of the hand
        Quaternion rotationCorrection = Quaternion.FromToRotation(inAxis, upAxis);

        switch (alignment)
        {
            default:
            case AvatarAlignment.Center:
                position = wristPosition;
                rotation = Quaternion.FromToRotation(upAxis, fingerAxis) * (tiltOffset * (rotationCorrection * wristRotation));
                break;
            case AvatarAlignment.Back:
                position = wristPosition + upAxis * wristUpRadius;
                rotation = tiltOffset * (rotationCorrection * wristRotation);
                break;
            case AvatarAlignment.Front:
                position = wristPosition - upAxis * wristUpRadius;
                rotation = tiltOffset * (Quaternion.AngleAxis(180f, fingerAxis) * (rotationCorrection * wristRotation));
                break;
            case AvatarAlignment.Out:
                position = wristPosition - inAxis * wristSideRadius;
                rotation = tiltOffset * (Quaternion.AngleAxis(180f, fingerAxis) * wristRotation);
                break;
            case AvatarAlignment.In:
                position = wristPosition + inAxis * wristSideRadius;
                rotation = tiltOffset * wristRotation;
                break;
        }
    }

    private static void GetAnkleTransform(AvatarAlignment alignment, AvatarSide side, PhysicsRig physicsRig, ArtRig artRig, Avatar avatar, out Vector3 position, out Quaternion rotation)
    {
        bool left = side != AvatarSide.Right;
        var leftSign = left ? -1f : 1f;

        var eyeHeight = avatar.eyeHeight;

        Transform ankleTransform = left ? physicsRig.m_footLf : physicsRig.m_footRt;
        var anklePosition = ankleTransform.position;
        var ankleRotation = ankleTransform.rotation;

        var ankleForward = ankleTransform.forward;
        var ankleUp = ankleTransform.up;
        var ankleRight = ankleTransform.right;

        var ankleEllipse = avatar.ankleEllipse;

        var frontOffsetRotation = Quaternion.AngleAxis(90f, ankleRight) * ankleRotation;

        switch (alignment)
        {
            default:
            case AvatarAlignment.Center:
                position = anklePosition;
                rotation = ankleRotation;
                break;
            case AvatarAlignment.Front:
                position = anklePosition + eyeHeight * ankleEllipse.ZRadius * ankleForward;
                rotation = frontOffsetRotation;
                break;
            case AvatarAlignment.Back:
                position = anklePosition - eyeHeight * ankleEllipse.ZRadius * ankleForward;
                rotation = Quaternion.AngleAxis(180f, ankleUp) * frontOffsetRotation;
                break;
            case AvatarAlignment.Out:
                position = anklePosition + leftSign * eyeHeight * ankleEllipse.XRadius * ankleRight;
                rotation = Quaternion.AngleAxis(90f * leftSign, ankleUp) * frontOffsetRotation;
                break;
            case AvatarAlignment.In:
                position = anklePosition - leftSign * eyeHeight * ankleEllipse.XRadius * ankleRight;
                rotation = Quaternion.AngleAxis(-90f * leftSign, ankleUp) * frontOffsetRotation;
                break;
        }
    }

    private static Vector3 GetEyeCenter(ArtRig artRig)
    {
        return (artRig.eyeLf.position + artRig.eyeRt.position) * 0.5f;
    }

    private static float GetHeadZOffset(Avatar avatar) => (avatar.ForeheadEllipseZ - avatar.ForeheadEllipseNegZ) * 0.5f;

    private static Vector3 GetPositionWithRelativeY(Transform origin, Vector3 xzPosition, Vector3 yPosition, float yOffset)
    {
        var localXZPosition = origin.InverseTransformPoint(xzPosition);
        var localYPosition = origin.InverseTransformPoint(yPosition);

        var localPosition = localXZPosition;
        localPosition.y = localYPosition.y + yOffset;

        var position = origin.TransformPoint(localPosition);

        return position;
    }
}
