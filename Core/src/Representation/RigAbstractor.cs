using SLZ.Rig;

using UnityEngine;

namespace LabFusion.Representation
{
    /// <summary>
    /// Helper class for abstracting synced transforms between rigs for easy updating.
    /// </summary>
    public static class RigAbstractor
    {
        public const int TransformSyncCount = 5;

        public static Transform GetSmoothTurnTransform(this RigManager manager)
        {
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
    }
}
