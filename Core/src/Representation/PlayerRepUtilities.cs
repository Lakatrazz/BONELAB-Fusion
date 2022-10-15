using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SLZ.Rig;

using UnhollowerRuntimeLib;

using UnityEngine;

using LabFusion.Utilities;
using LabFusion.Data;
using LabFusion.Network;

using SLZ;
using SLZ.Interaction;

namespace LabFusion.Representation {
    public static class PlayerRepUtilities {
        public const int TransformSyncCount = 3;

        public const string PolyBlankBarcode = "c3534c5a-94b2-40a4-912a-24a8506f6c79";

        public static bool FindAttachedPlayerRep(Grip grip, out PlayerRep rep) {
            rep = null;

            if (grip == null)
                return false;

            var rig = RigManager.Cache.Get(grip.transform.root.gameObject);
            if (rig && PlayerRep.Managers.ContainsKey(rig))
            {
                rep = PlayerRep.Managers[rig];
                return true;
            }
            else {
#if DEBUG
                FusionLogger.Warn("Failed to find parent rig of grip!");
#endif

                return false;
            }
        }

        public static void SendObjectAttach(Handedness handedness, Grip grip) { 
            if (NetworkUtilities.HasServer) {
                // Get base values for the message
                byte smallId = PlayerId.SelfId.SmallId;
                SyncUtilities.SyncGroup group = SyncUtilities.SyncGroup.UNKNOWN;
                SerializedGrab serializedGrab = null;
                bool validGrip = false;

                // If the grip exists, we'll check its stuff
                if (grip != null) {
                    // Check for player body grab
                    if (FindAttachedPlayerRep(grip, out var rep)) {
                        group = SyncUtilities.SyncGroup.PLAYER_BODY;
                        serializedGrab = new SerializedPlayerBodyGrab(rep.RigReferences.GetIndex(grip).Value);
                        validGrip = true;
                    }
                }

                // Now, send the message
                if (validGrip) {
                    using (var writer = FusionWriter.Create()) {
                        using (var data = PlayerRepGrabData.Create(smallId, handedness, group, serializedGrab)) {
                            writer.Write(data);

                            using (var message = FusionMessage.Create(NativeMessageTag.PlayerRepGrab, writer)) {
                                NetworkUtilities.BroadcastMessage(NetworkChannel.Reliable, message);
                            }
                        }
                    }
                }
            }
        }

        public static void SendObjectDetach(Handedness handedness) {
            if (NetworkUtilities.HasServer) {
                using (var writer = FusionWriter.Create()) {
                    using (var data = PlayerRepReleaseData.Create(PlayerId.SelfId.SmallId, handedness)) {
                        writer.Write(data);

                        using (var message = FusionMessage.Create(NativeMessageTag.PlayerRepRelease, writer)) {
                            NetworkUtilities.BroadcastMessage(NetworkChannel.Reliable, message);
                        }
                    }
                }
            }
        }

        public static RigManager CreateNewRig() {
            var go = GameObject.Instantiate(AssetBundleManager.PlayerRepBundle.LoadAsset(ResourcePaths.PlayerRepName, Il2CppType.Of<GameObject>())).Cast<GameObject>();

            if (RigData.RigReferences.RigManager) {
                go.transform.position = RigData.RigSpawn;
                go.transform.rotation = RigData.RigSpawnRot;
            }

            return go.GetComponent<RigManager>();
        }

        public static void FillTransformArray(ref Transform[] array, RigManager manager) {
            var rig = manager.openControllerRig;

            array[0] = rig.m_head;
            array[1] = rig.m_handLf;
            array[2] = rig.m_handRt;
        }
    }
}
