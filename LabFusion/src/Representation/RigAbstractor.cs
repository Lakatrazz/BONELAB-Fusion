using Il2CppSLZ.Rig;

using UnityEngine;

namespace LabFusion.Representation
{
    /// <summary>
    /// Helper class for abstracting synced transforms between rigs for easy updating.
    /// </summary>
    public static class RigAbstractor
    {
        public const int TransformSyncCount = 3;

        public static Transform GetSmoothTurnTransform(this RigManager manager)
        {
            return manager.remapHeptaRig.transform;
        }

        public static void FillTransformArray(ref Transform[] array, RigManager manager)
        {
            array = new Transform[TransformSyncCount];

            var rig = manager.ControllerRig.TryCast<OpenControllerRig>();

            array[0] = rig.headset;
            array[1] = rig.m_handLf;
            array[2] = rig.m_handRt;
        }
    }
}
