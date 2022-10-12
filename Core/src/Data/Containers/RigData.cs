using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MelonLoader;

using SLZ.Rig;
using SLZ.Interaction;

using BoneLib;

using UnityEngine;
using SLZ.Marrow.Utilities;
using SLZ.Marrow.Warehouse;
using LabFusion.Utilities;
using UnhollowerRuntimeLib;
using LabFusion.Network;
using LabFusion.Representation;
using SLZ.VRMK;
using SLZ;

namespace LabFusion.Data
{
    /// <summary>
    /// A collection of basic rig information for use across PlayerReps and the Main RigManager.
    /// </summary>
    public struct RigReferenceCollection {
        public RigManager RigManager { get; private set; }

        public Grip[] RigGrips { get; private set; }

        public Hand LeftHand { get; private set; }
        public Hand RightHand { get; private set; }

        public BaseController LeftController { get; private set; }
        public BaseController RightController { get; private set; }

        public byte? GetIndex(Grip grip) {
            for (byte i = 0; i < RigGrips.Length; i++) {
                if (RigGrips[i] == grip)
                    return i;
            }
            return null;
        }

        public Grip GetGrip(byte index) {
            if (RigGrips.Length > index)
                return RigGrips[index];
            return null;
        }

        public Hand GetHand(Handedness hand) {
            switch (hand) {
                default:
                    return LeftHand;
                case Handedness.RIGHT:
                    return RightHand;
            }
        }

        public RigReferenceCollection(RigManager rigManager) {
            RigManager = rigManager;
            RigGrips = rigManager.physicsRig.GetComponentsInChildren<Grip>(true);

            LeftHand = rigManager.physicsRig.m_handLf.GetComponent<Hand>();
            RightHand = rigManager.physicsRig.m_handRt.GetComponent<Hand>();

            LeftController = rigManager.openControllerRig.leftController;
            RightController = rigManager.openControllerRig.rightController;
        }
    }

    public static class RigData
    {
        public static RigReferenceCollection RigReferences { get; private set; }

        public static string RigScene { get; private set; }
        public static string RigAvatarId { get; private set; } = NetworkUtilities.InvalidAvatarId;

        public static Vector3 RigSpawn { get; private set; }
        public static Quaternion RigSpawnRot { get; private set; }

        public static void OnCacheRigInfo(string sceneName) {
            var rigObject = Player.GetRigManager();

            if (!rigObject) {
                RigScene = null;
                return;
            }
            
            RigScene = sceneName;

            RigReferences = new RigReferenceCollection(rigObject.GetComponent<RigManager>());
            RigReferences.RigManager.bodyVitals.rescaleEvent += (BodyVitals.RescaleUI)OnRigRescale;

            RigSpawn = rigObject.transform.position;
            RigSpawnRot = rigObject.transform.rotation;
        }

        public static void OnRigRescale() {
            // Send body vitals to network
            if (NetworkUtilities.HasServer) {
                using (FusionWriter writer = FusionWriter.Create()) {
                    using (PlayerRepVitalsData data = PlayerRepVitalsData.Create(PlayerId.SelfId.SmallId, RigReferences.RigManager.bodyVitals)) {
                        writer.Write(data);

                        using (var message = FusionMessage.Create(NativeMessageTag.PlayerRepVitals, writer)) {
                            FusionMod.CurrentNetworkLayer.BroadcastMessage(NetworkChannel.Reliable, message);
                        }
                    }
                }
            }
        }

        public static void OnRigUpdate() {
            if (RigReferences.RigManager) {
                var barcode = GetAvatarBarcode();
                if (barcode != RigAvatarId) {
#if DEBUG
                    FusionLogger.Log($"Local avatar switched from {RigAvatarId} to {barcode}!");
#endif

                    // Send switch message to notify the server
                    if (NetworkUtilities.HasServer) {
                        using (FusionWriter writer = FusionWriter.Create()) {
                            using (PlayerRepAvatarData data = PlayerRepAvatarData.Create(PlayerId.SelfId.SmallId, barcode)) {
                                writer.Write(data);

                                using (var message = FusionMessage.Create(NativeMessageTag.PlayerRepAvatar, writer)) {
                                    FusionMod.CurrentNetworkLayer.BroadcastMessage(NetworkChannel.Reliable, message);
                                }
                            }
                        }
                    }
                }
                RigAvatarId = barcode;
            }
        }

        public static string GetAvatarBarcode() {
            if (RigReferences.RigManager)
                return RigReferences.RigManager.AvatarCrate.Barcode;
            return NetworkUtilities.InvalidAvatarId;
        }
    }
}
