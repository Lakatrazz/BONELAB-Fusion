using SLZ.Rig;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.Representation {
    /// <summary>
    /// Helper class for abstracting synced transforms between rigs for easy updating.
    /// </summary>
    public static class RigAbstractor
    {
        public const int TransformSyncCount = 5;
        public const int GameworldRigTransformCount = 10;

        public static Transform GetSmoothTurnTransform(this RigManager manager) {
            return manager.remapHeptaRig.transform;
        }

        public static void FillTransformArray(ref Transform[] array, RigManager manager)
        {
            array = new Transform[TransformSyncCount];

            var rig = manager.openControllerRig;

            array[0] = rig.m_head;
            array[1] = rig.m_handLf;
            array[2] = rig.m_handRt;
            array[3] = rig.leftController.transform;
            array[4] = rig.rightController.transform;
        }

        public static void FillGameworldArray(ref Transform[] array, RigManager manager)
        {
            array = new Transform[GameworldRigTransformCount];

            var rig = manager.virtualHeptaRig;

            array[0] = rig.m_head;
            array[1] = rig.m_chest;
            array[2] = rig.m_spine;
            array[3] = rig.m_pelvis;
            array[4] = rig.m_shoulderLf;
            array[5] = rig.m_elbowLf;
            array[6] = rig.m_handLf;
            array[7] = rig.m_shoulderRt;
            array[8] = rig.m_elbowRt;
            array[9] = rig.m_handRt;
        }
    }
}
