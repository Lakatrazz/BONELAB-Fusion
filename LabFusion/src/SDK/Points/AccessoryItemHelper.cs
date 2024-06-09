using LabFusion.Extensions;
using LabFusion.MarrowIntegration;
using LabFusion.Utilities;
using Il2CppSLZ.Rig;

using UnityEngine;

using Avatar = Il2CppSLZ.VRMK.Avatar;

namespace LabFusion.SDK.Points
{
    public enum AccessoryPoint
    {
        // Head
        HEAD,
        HEAD_TOP,

        // Eyes
        EYE_LEFT,
        EYE_RIGHT,
        EYE_CENTER,

        // Face
        NOSE,

        // Spine
        CHEST,
        CHEST_BACK,
        HIPS,
        LOCOSPHERE,
    }

    public enum AccessoryScaleMode
    {
        NONE,
        HEIGHT,
        HEAD,
    }

    public static class AccessoryItemHelper
    {
        private static Vector3 GetEyeCenter(RigManager rig, ArtRig artRig)
        {
            if (TimeUtilities.TimeScale > 0f)
            {
                return rig.ControllerRig.m_head.position;
            }
            else
            {
                return (artRig.eyeLf.position + artRig.eyeRt.position) * 0.5f;
            }
        }

        public static void GetTransform(MarrowCosmeticPoint point, out Vector3 position, out Quaternion rotation, out Vector3 scale)
        {
            position = point.Transform.position;
            rotation = point.Transform.rotation;
            scale = point.Transform.lossyScale;
        }

        public static void GetTransform(AccessoryPoint itemPoint, AccessoryScaleMode mode, RigManager rig, out Vector3 position, out Quaternion rotation, out Vector3 scale)
        {
            PhysicsRig inRig = rig.physicsRig;
            ArtRig artRig = inRig.artOutput;
            Avatar avatar = rig._avatar;

            scale = GetScale(avatar, mode);

            var head = inRig.m_head;

            switch (itemPoint)
            {
                default:
                case AccessoryPoint.HEAD:
                    position = head.position;
                    rotation = head.rotation;
                    break;
                case AccessoryPoint.HEAD_TOP:
                    Vector3 eyeCenter = GetEyeCenter(rig, artRig);

                    eyeCenter += head.up * (avatar.HeadTop * 1.5f * avatar.height);
                    eyeCenter = head.InverseTransformPoint(eyeCenter);
                    
                    position = head.position;
                    position = head.InverseTransformPoint(position);

                    position.y = eyeCenter.y;
                    position = head.TransformPoint(position);

                    rotation = head.rotation;
                    break;
                case AccessoryPoint.EYE_LEFT:
                    position = artRig.eyeLf.position;
                    rotation = artRig.eyeLf.rotation;
                    break;
                case AccessoryPoint.EYE_CENTER:
                    position = GetEyeCenter(rig, artRig);
                    rotation = inRig.m_head.rotation;
                    break;
                case AccessoryPoint.NOSE:
                    Vector3 noseCenter = GetEyeCenter(rig, artRig);
                    position = inRig.m_head.position + inRig.m_head.forward * (avatar.ForeheadEllipseZ * avatar.height);

                    noseCenter = inRig.m_head.InverseTransformPoint(noseCenter);
                    position = inRig.m_head.InverseTransformPoint(position);

                    position.y = noseCenter.y;

                    position = inRig.m_head.TransformPoint(position);

                    rotation = inRig.m_head.rotation;
                    break;
                case AccessoryPoint.EYE_RIGHT:
                    position = artRig.eyeRt.position;
                    rotation = artRig.eyeRt.rotation;
                    break;
                case AccessoryPoint.CHEST:
                    position = inRig.m_chest.position;
                    rotation = inRig.m_chest.rotation;
                    break;
                case AccessoryPoint.CHEST_BACK:
                    Transform chest = inRig.m_chest;
                    position = chest.position - chest.forward * avatar.ChestEllipseNegZ;
                    rotation = chest.rotation;
                    break;
                case AccessoryPoint.HIPS:
                    position = inRig.m_pelvis.position;
                    rotation = inRig.m_pelvis.rotation;
                    break;
                case AccessoryPoint.LOCOSPHERE:
                    Transform physG = rig.physicsRig.physG.transform;
                    position = physG.position;
                    rotation = physG.rotation;
                    break;
            }
        }

        public static Vector3 GetScale(Avatar avatar, AccessoryScaleMode mode)
        {
            switch (mode)
            {
                default:
                case AccessoryScaleMode.NONE:
                    return Vector3Extensions.one;
                case AccessoryScaleMode.HEIGHT:
                    return Vector3Extensions.one * (avatar.height / 1.76f);
                case AccessoryScaleMode.HEAD:
                    return Vector3Extensions.one * (avatar.ForeheadEllipseX / 0.044f * avatar.height) / 1.76f;
            }
        }
    }
}
