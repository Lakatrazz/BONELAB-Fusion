using LabFusion.Data;
using LabFusion.Network;
using Il2CppSLZ.Interaction;

using UnityEngine;
using LabFusion.Utilities;

namespace LabFusion.Representation
{
    public static class LocalPlayer
    {
        public static Transform[] syncedPoints = null;
        public static Transform syncedPlayspace;
        public static Transform syncedPelvis;
        public static Hand syncedLeftHand;
        public static Hand syncedRightHand;

        private static bool TrySendRep()
        {
            try
            {
                if (syncedPoints == null || PlayerIdManager.LocalId == null)
                    return false;

                using var writer = FusionWriter.Create(PlayerRepTransformData.Size);
                var data = PlayerRepTransformData.Create(PlayerIdManager.LocalSmallId, syncedPoints, syncedPelvis, syncedPlayspace, syncedLeftHand, syncedRightHand);
                writer.Write(data);

                using var message = FusionMessage.Create(NativeMessageTag.PlayerRepTransform, writer);
                MessageSender.BroadcastMessageExceptSelf(NetworkChannel.Unreliable, message);

                return true;
            }
            catch (Exception e)
            {
#if DEBUG
                FusionLogger.Error($"Failed sending player transforms with reason: {e.Message}\nTrace:{e.StackTrace}");
#endif
            }
            return false;
        }

        public static void OnSyncRep()
        {
            if (NetworkInfo.HasServer && RigData.HasPlayer)
            {
                if (!TrySendRep())
                    OnCachePlayerTransforms();
            }
            else
            {
                syncedPoints = null;
            }
        }

        public static void OnCachePlayerTransforms()
        {
            if (!RigData.HasPlayer)
                return;

            var rm = RigData.RigReferences.RigManager;
            syncedPelvis = rm.physicsRig.m_pelvis;
            syncedPlayspace = rm.GetSmoothTurnTransform();
            syncedLeftHand = rm.physicsRig.leftHand;
            syncedRightHand = rm.physicsRig.rightHand;

            RigAbstractor.FillTransformArray(ref syncedPoints, rm);
        }
    }
}
