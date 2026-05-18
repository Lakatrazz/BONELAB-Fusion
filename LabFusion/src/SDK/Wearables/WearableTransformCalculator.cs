using LabFusion.Extensions;
using LabFusion.Marrow.Integration;
using LabFusion.Marrow;

using Il2CppSLZ.Marrow;

using UnityEngine;

using Avatar = Il2CppSLZ.VRMK.Avatar;

namespace LabFusion.SDK.Wearables;

public static class WearableTransformCalculator
{
    private static Vector3 GetEyeCenter(ArtRig artRig)
    {
        return (artRig.eyeLf.position + artRig.eyeRt.position) * 0.5f;
    }

    private static Vector3 GetHeadCenter(PhysicsRig physicsRig, ArtRig artRig, Avatar avatar)
    {
        var physicsHead = physicsRig.m_head;
        var artHead = artRig.artHead;

        var headForward = physicsHead.forward;
        float offset = avatar.eyeHeight * (avatar.ForeheadEllipseZ - avatar.ForeheadEllipseNegZ) * 0.5f;

        return artHead.position + offset * headForward;
    }

    public static WearableScaleMode GetScaleMode(WearablePoint point)
    {
        return point switch
        {
            WearablePoint.Head or 
            WearablePoint.HeadTop or 
            WearablePoint.EyeRight or 
            WearablePoint.EyeLeft or 
            WearablePoint.EyeCenter or 
            WearablePoint.Nose => WearableScaleMode.Head,
            WearablePoint.WristLeft or
            WearablePoint.WristLeftTop or
            WearablePoint.WristLeftBottom or
            WearablePoint.WristLeftOut or
            WearablePoint.WristLeftIn or
            WearablePoint.WristRight or
            WearablePoint.WristRightTop or
            WearablePoint.WristRightBottom or
            WearablePoint.WristRightOut or
            WearablePoint.WristRightIn => WearableScaleMode.Wrist,
            _ => WearableScaleMode.Height,
        };
    }

    public static void GetTransform(AvatarCosmeticPoint point, out Vector3 position, out Quaternion rotation, out Vector3 scale)
    {
        var transform = point.transform;
        position = transform.position;
        rotation = transform.rotation;
        scale = transform.lossyScale;
    }

    public static void GetTransform(WearablePoint point, RigManager rigManager, out Vector3 position, out Quaternion rotation, out Vector3 scale)
    {
        PhysicsRig physicsRig = rigManager.physicsRig;
        ArtRig artRig = physicsRig.artOutput;
        Avatar avatar = rigManager.avatar;

        scale = GetScale(avatar, GetScaleMode(point));

        var head = physicsRig.m_head;

        switch (point)
        {
            default:
            case WearablePoint.Head:
                position = GetHeadCenter(physicsRig, artRig, avatar);
                rotation = head.rotation;
                break;
            case WearablePoint.HeadTop:
                var eyeCenter = GetEyeCenter(artRig);
                var headCenter = GetHeadCenter(physicsRig, artRig, avatar);

                float upOffset = avatar.eyeHeight * avatar.HeadTop;

                var localEyeCenter = head.InverseTransformPoint(eyeCenter);
                var localHeadTop = head.InverseTransformPoint(headCenter);

                localHeadTop.y = localEyeCenter.y + upOffset;

                var headTop = head.TransformPoint(localHeadTop);

                position = headTop;
                rotation = head.rotation;
                break;
            case WearablePoint.EyeLeft:
                position = artRig.eyeLf.position;
                rotation = artRig.eyeLf.rotation;
                break;
            case WearablePoint.EyeCenter:
                position = GetEyeCenter(artRig);
                rotation = head.rotation;
                break;
            case WearablePoint.Nose:
                Vector3 noseCenter = GetEyeCenter(artRig);
                position = head.position + head.forward * (avatar.ForeheadEllipseZ * avatar.height);

                noseCenter = head.InverseTransformPoint(noseCenter);
                position = head.InverseTransformPoint(position);

                position.y = noseCenter.y;

                position = head.TransformPoint(position);

                rotation = head.rotation;
                break;
            case WearablePoint.EyeRight:
                position = artRig.eyeRt.position;
                rotation = artRig.eyeRt.rotation;
                break;
            case WearablePoint.Chest:
                position = physicsRig.m_chest.position;
                rotation = physicsRig.m_chest.rotation;
                break;
            case WearablePoint.ChestBack:
                Transform chest = physicsRig.m_chest;
                position = chest.position - chest.forward * avatar.ChestEllipseNegZ;
                rotation = chest.rotation;
                break;
            case WearablePoint.Hips:
                position = physicsRig.m_pelvis.position;
                rotation = physicsRig.m_pelvis.rotation;
                break;
            case WearablePoint.WristLeft:
            case WearablePoint.WristLeftTop:
            case WearablePoint.WristLeftBottom:
            case WearablePoint.WristLeftOut:
            case WearablePoint.WristLeftIn:
            case WearablePoint.WristRight:
            case WearablePoint.WristRightTop:
            case WearablePoint.WristRightBottom:
            case WearablePoint.WristRightOut:
            case WearablePoint.WristRightIn:
                GetWristTransform(point, artRig, avatar, out position, out rotation);
                break;
        }
    }

    private static void GetWristTransform(WearablePoint wristPoint, ArtRig artRig, Avatar avatar, out Vector3 position, out Quaternion rotation)
    {
        bool left = wristPoint switch
        {
            WearablePoint.WristLeft or
            WearablePoint.WristLeftTop or
            WearablePoint.WristLeftBottom or
            WearablePoint.WristLeftOut or
            WearablePoint.WristLeftIn => true,
            _ => false
        };

        WearableSide side = wristPoint switch
        {
            WearablePoint.WristLeftTop or
            WearablePoint.WristRightTop => WearableSide.Back,
            WearablePoint.WristLeftBottom or
            WearablePoint.WristRightBottom => WearableSide.Front,
            WearablePoint.WristLeftOut or
            WearablePoint.WristRightOut => WearableSide.Out,
            WearablePoint.WristLeftIn or
            WearablePoint.WristRightIn => WearableSide.In,
            _ => WearableSide.Center,
        };

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

        switch (side)
        {
            case WearableSide.In:
            case WearableSide.Out:
                tiltAngle = 0f;
                break;
            case WearableSide.Back:
                tiltAngle *= -1f;
                break;
        }

        var tiltOffset = Quaternion.AngleAxis(tiltAngle, inAxis);

        // Offset to correct the given rotation so that forward is the direction of the fingers and up is out from the back of the hand
        Quaternion rotationCorrection = Quaternion.FromToRotation(inAxis, upAxis);

        switch (wristPoint)
        {
            default:
            case WearablePoint.WristLeft:
            case WearablePoint.WristRight:
                position = wristPosition;
                rotation = tiltOffset * (rotationCorrection * wristRotation);
                break;
            case WearablePoint.WristLeftTop:
            case WearablePoint.WristRightTop:
                position = wristPosition + upAxis * wristUpRadius;
                rotation = tiltOffset * (rotationCorrection * wristRotation);
                break;
            case WearablePoint.WristLeftBottom:
            case WearablePoint.WristRightBottom:
                position = wristPosition - upAxis * wristUpRadius;
                rotation = tiltOffset * (Quaternion.AngleAxis(180f, fingerAxis) * (rotationCorrection * wristRotation));
                break;
            case WearablePoint.WristLeftOut:
            case WearablePoint.WristRightOut:
                position = wristPosition - inAxis * wristSideRadius;
                rotation = tiltOffset * (Quaternion.AngleAxis(180f, fingerAxis) * wristRotation);
                break;
            case WearablePoint.WristLeftIn:
            case WearablePoint.WristRightIn:
                position = wristPosition + inAxis * wristSideRadius;
                rotation = tiltOffset * wristRotation;
                break;
        }
    }

    public static Vector3 GetScale(Avatar avatar, WearableScaleMode mode)
    {
        var scale = Vector3Extensions.One;
        float heightProportion = avatar.height / MarrowConstants.StandardHeight;

        switch (mode)
        {
            case WearableScaleMode.Height:
                scale *= heightProportion;
                break;
            case WearableScaleMode.Head:
                scale *= avatar.ForeheadEllipseX / 0.044f * heightProportion;
                break;
            case WearableScaleMode.Wrist:
                var wristEllipse = avatar.wristEllipse;
                var averageRadius = (wristEllipse.XRadius + wristEllipse.ZRadius) * 0.5f;
                float referenceRadius = 0.01985f;

                scale *= averageRadius / referenceRadius * heightProportion;
                break;
        }

        return scale;
    }
}
