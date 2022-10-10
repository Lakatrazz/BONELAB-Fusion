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

namespace LabFusion.Data
{
    public static class RigData
    {
        public static RigManager RigManager { get; private set; }

        public static Hand LeftHand { get; private set; }
        public static Hand RightHand { get; private set; }

        public static BaseController LeftController { get; private set; }
        public static BaseController RightController { get; private set; }

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

            RigManager = rigObject.GetComponent<RigManager>();
            RigManager.bodyVitals.rescaleEvent += (BodyVitals.RescaleUI)OnRigRescale;

            RigSpawn = RigManager.transform.position;
            RigSpawnRot = RigManager.transform.rotation;

            LeftHand = Player.leftHand;
            RightHand = Player.rightHand;

            LeftController = Player.leftController;
            RightController = Player.rightController;
        }

        public static void OnRigRescale() {
            // Send body vitals to network
            if (NetworkUtilities.HasServer) {
                using (FusionWriter writer = FusionWriter.Create()) {
                    using (PlayerRepVitalsData data = PlayerRepVitalsData.Create(PlayerId.SelfId.SmallId, RigManager.bodyVitals)) {
                        writer.Write(data);

                        using (var message = FusionMessage.Create(NativeMessageTag.PlayerRepVitals, writer)) {
                            FusionMod.CurrentNetworkLayer.BroadcastMessage(NetworkChannel.Reliable, message);
                        }
                    }
                }
            }
        }

        public static void OnRigUpdate() {
            if (RigManager) {
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
            if (RigManager)
                return RigManager.AvatarCrate.Barcode;
            return NetworkUtilities.InvalidAvatarId;
        }
    }
}
