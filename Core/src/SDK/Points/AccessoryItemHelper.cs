using LabFusion.Extensions;
using SLZ.Rig;
using SLZ.VRMK;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

using Avatar = SLZ.VRMK.Avatar;

namespace LabFusion.SDK.Points {
    public enum AccessoryPoint
    {
        HEAD,
        HEAD_TOP,
        EYE_LEFT,
        EYE_RIGHT,
        EYE_CENTER,
        NOSE,
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

    public static class AccessoryItemHelper {
        public static void GetTransform(AccessoryPoint itemPoint, AccessoryScaleMode mode, RigManager rig, out Vector3 position, out Quaternion rotation, out Vector3 scale) {
            ArtRig artRig = rig.artOutputRig;
            Avatar avatar = rig._avatar;

            scale = GetScale(avatar, mode);

            switch (itemPoint)
            {
                default:
                case AccessoryPoint.HEAD:
                    position = artRig.m_head.position;
                    rotation = artRig.m_head.rotation;
                    break;
                case AccessoryPoint.HEAD_TOP:
                    Vector3 eyeCenter = (artRig.eyeLf.position + artRig.eyeRt.position) * 0.5f;
                    eyeCenter += artRig.m_head.up * (avatar._headTop * avatar.height);
                    eyeCenter = artRig.m_head.InverseTransformPoint(eyeCenter);

                    position = artRig.m_head.position;
                    position = artRig.m_head.InverseTransformPoint(position);

                    position.y = eyeCenter.y;
                    position = artRig.m_head.TransformPoint(position);

                    rotation = artRig.m_head.rotation;
                    break;
                case AccessoryPoint.EYE_LEFT:
                    position = artRig.eyeLf.position;
                    rotation = artRig.eyeLf.rotation;
                    break;
                case AccessoryPoint.EYE_CENTER:
                    position = (artRig.eyeLf.position + artRig.eyeRt.position) * 0.5f;
                    rotation = artRig.m_head.rotation;
                    break;
                case AccessoryPoint.NOSE:
                    Vector3 noseCenter = (artRig.eyeLf.position + artRig.eyeRt.position) * 0.5f;
                    position = artRig.m_head.position + artRig.m_head.forward * (avatar.ForeheadEllipseZ * avatar.height);

                    noseCenter = artRig.m_head.InverseTransformPoint(noseCenter);
                    position = artRig.m_head.InverseTransformPoint(position);

                    position.y = noseCenter.y;

                    position = artRig.m_head.TransformPoint(position);

                    rotation = artRig.m_head.rotation;
                    break;
                case AccessoryPoint.EYE_RIGHT:
                    position = artRig.eyeRt.position;
                    rotation = artRig.eyeRt.rotation;
                    break;
                case AccessoryPoint.CHEST:
                    position = artRig.m_chest.position;
                    rotation = artRig.m_chest.rotation;
                    break;
                case AccessoryPoint.CHEST_BACK:
                    Transform chest = artRig.m_chest;
                    position = chest.position - chest.forward * avatar.ChestEllipseNegZ;
                    rotation = chest.rotation;
                    break;
                case AccessoryPoint.HIPS:
                    position = artRig.m_pelvis.position;
                    rotation = artRig.m_pelvis.rotation;
                    break;
                case AccessoryPoint.LOCOSPHERE:
                    Transform physG = rig.physicsRig.physG.transform;
                    position = physG.position;
                    rotation = physG.rotation;
                    break;
            }
        }

        public static Vector3 GetScale(Avatar avatar, AccessoryScaleMode mode) {
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
