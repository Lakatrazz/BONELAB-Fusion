using LabFusion.Extensions;
using LabFusion.Marrow.Integration;
using LabFusion.Marrow;

using Il2CppSLZ.Marrow;

using UnityEngine;

using Avatar = Il2CppSLZ.VRMK.Avatar;

namespace LabFusion.SDK.Cosmetics;

public enum CosmeticScaleMode
{
    NONE,
    HEIGHT,
    HEAD,
}

public static class CosmeticItemHelper
{
    private static Vector3 GetEyeCenter(ArtRig artRig)
    {
        return (artRig.eyeLf.position + artRig.eyeRt.position) * 0.5f;
    }

    public static CosmeticScaleMode GetScaleMode(RigPoint point)
    {
        switch (point)
        {
            default:
            case RigPoint.CHEST:
            case RigPoint.CHEST_BACK:
            case RigPoint.LOCOSPHERE:
            case RigPoint.HIPS:
                return CosmeticScaleMode.HEIGHT;
            case RigPoint.HEAD:
            case RigPoint.HEAD_TOP:
            case RigPoint.EYE_RIGHT:
            case RigPoint.EYE_LEFT:
            case RigPoint.EYE_CENTER:
            case RigPoint.NOSE:
                return CosmeticScaleMode.HEAD;
        }
    }

    public static void GetTransform(AvatarCosmeticPoint point, out Vector3 position, out Quaternion rotation, out Vector3 scale)
    {
        var transform = point.transform;
        position = transform.position;
        rotation = transform.rotation;
        scale = transform.lossyScale;
    }

    public static void GetTransform(RigPoint itemPoint, RigManager rig, out Vector3 position, out Quaternion rotation, out Vector3 scale)
    {
        PhysicsRig inRig = rig.physicsRig;
        ArtRig artRig = inRig.artOutput;
        Avatar avatar = rig._avatar;

        scale = GetScale(avatar, GetScaleMode(itemPoint));

        var head = inRig.m_head;

        switch (itemPoint)
        {
            default:
            case RigPoint.HEAD:
                position = head.position;
                rotation = head.rotation;
                break;
            case RigPoint.HEAD_TOP:
                Vector3 eyeCenter = GetEyeCenter(artRig);

                eyeCenter += head.up * (avatar.HeadTop * avatar.height);
                eyeCenter -= head.forward * (avatar.ForeheadEllipseZ * avatar.height * 0.5f);

                eyeCenter = head.InverseTransformPoint(eyeCenter);

                position = head.position;
                position = head.InverseTransformPoint(position);

                position.y = eyeCenter.y;
                position = head.TransformPoint(position);

                rotation = head.rotation;
                break;
            case RigPoint.EYE_LEFT:
                position = artRig.eyeLf.position;
                rotation = artRig.eyeLf.rotation;
                break;
            case RigPoint.EYE_CENTER:
                position = GetEyeCenter(artRig);
                rotation = head.rotation;
                break;
            case RigPoint.NOSE:
                Vector3 noseCenter = GetEyeCenter(artRig);
                position = head.position + head.forward * (avatar.ForeheadEllipseZ * avatar.height);

                noseCenter = head.InverseTransformPoint(noseCenter);
                position = head.InverseTransformPoint(position);

                position.y = noseCenter.y;

                position = head.TransformPoint(position);

                rotation = head.rotation;
                break;
            case RigPoint.EYE_RIGHT:
                position = artRig.eyeRt.position;
                rotation = artRig.eyeRt.rotation;
                break;
            case RigPoint.CHEST:
                position = inRig.m_chest.position;
                rotation = inRig.m_chest.rotation;
                break;
            case RigPoint.CHEST_BACK:
                Transform chest = inRig.m_chest;
                position = chest.position - chest.forward * avatar.ChestEllipseNegZ;
                rotation = chest.rotation;
                break;
            case RigPoint.HIPS:
                position = inRig.m_pelvis.position;
                rotation = inRig.m_pelvis.rotation;
                break;
            case RigPoint.LOCOSPHERE:
                Transform physG = rig.physicsRig.physG.transform;
                position = physG.position;
                rotation = physG.rotation;
                break;
        }
    }

    public static Vector3 GetScale(Avatar avatar, CosmeticScaleMode mode)
    {
        return mode switch
        {
            CosmeticScaleMode.HEIGHT => Vector3Extensions.one * (avatar.height / MarrowConstants.StandardHeight),
            CosmeticScaleMode.HEAD => Vector3Extensions.one * (avatar.ForeheadEllipseX / 0.044f * avatar.height) / MarrowConstants.StandardHeight,
            _ => Vector3Extensions.one,
        };
    }
}