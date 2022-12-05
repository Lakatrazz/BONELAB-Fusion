using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MelonLoader;


using SLZ.VRMK;
using SLZ;
using SLZ.Rig;
using SLZ.Interaction;

using BoneLib;

using UnityEngine;

using LabFusion.Utilities;
using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.Extensions;

namespace LabFusion.Data
{
    /// <summary>
    /// A collection of basic rig information for use across PlayerReps and the Main RigManager.
    /// </summary>
    public class RigReferenceCollection {
        public RigManager RigManager { get; private set; }

        public Grip[] RigGrips { get; private set; }

        public InventorySlotReceiver[] RigSlots { get; private set; }

        public Hand LeftHand { get; private set; }
        public Hand RightHand { get; private set; }

        public BaseController LeftController { get; private set; }
        public BaseController RightController { get; private set; }

        public Grip LeftSnatchGrip { get; private set; }
        public Grip RightSnatchGrip { get; private set; }

        public SerializedGripAnchor LeftSerializedAnchor { get; private set; }
        public SerializedGripAnchor RightSerializedAnchor { get; private set; }

        public ConfigurableJoint LeftClientJoint { get; private set; }
        public ConfigurableJoint RightClientJoint { get; private set; }

        public SerializedGripAnchor GetSerializedAnchor(Handedness handedness)
        {
            switch (handedness)
            {
                default:
                    return LeftSerializedAnchor;
                case Handedness.RIGHT:
                    return RightSerializedAnchor;
            }
        }

        public void SetSerializedAnchor(Handedness handedness, SerializedGripAnchor anchor)
        {
            switch (handedness)
            {
                default:
                    LeftSerializedAnchor = anchor;
                    break;
                case Handedness.RIGHT:
                    RightSerializedAnchor = anchor;
                    break;
            }
        }


        public ConfigurableJoint GetClientJoint(Handedness handedness)
        {
            switch (handedness)
            {
                default:
                    return LeftClientJoint;
                case Handedness.RIGHT:
                    return RightClientJoint;
            }
        }

        public void SetClientJoint(Handedness handedness, ConfigurableJoint joint)
        {
            switch (handedness)
            {
                default:
                    LeftClientJoint = joint;
                    break;
                case Handedness.RIGHT:
                    RightClientJoint = joint;
                    break;
            }
        }

        public void RemoveJoint(Handedness handedness) {
            switch (handedness)
            {
                default:
                    if (LeftClientJoint)
                        GameObject.Destroy(LeftClientJoint);
                    break;
                case Handedness.RIGHT:
                    if (RightClientJoint)
                        GameObject.Destroy(RightClientJoint);
                    break;
            }
        }

        public Grip GetSnatch(Handedness handedness) {
            switch (handedness)
            {
                default:
                    return LeftSnatchGrip;
                case Handedness.RIGHT:
                    return RightSnatchGrip;
            }
        } 

        public void SetSnatch(Handedness handedness, Grip grip)
        {
            switch (handedness)
            {
                default:
                    LeftSnatchGrip = grip;
                    break;
                case Handedness.RIGHT:
                    RightSnatchGrip = grip;
                    break;
            }
        }

        public byte? GetIndex(Grip grip) {
            for (byte i = 0; i < RigGrips.Length; i++) {
                if (RigGrips[i] == grip)
                    return i;
            }
            return null;
        }

        public Grip GetGrip(byte index) {
            if (RigGrips != null && RigGrips.Length > index)
                return RigGrips[index];
            return null;
        }

        public byte? GetIndex(InventorySlotReceiver slot)
        {
            for (byte i = 0; i < RigSlots.Length; i++)
            {
                if (RigSlots[i] == slot)
                    return i;
            }
            return null;
        }

        public InventorySlotReceiver GetSlot(byte index) {
            if (RigSlots != null && RigSlots.Length > index)
                return RigSlots[index];
            return null;
        }

        public Hand GetHand(Handedness handedness) {
            switch (handedness) {
                default:
                    return null;
                case Handedness.LEFT:
                    return LeftHand;
                case Handedness.RIGHT:
                    return RightHand;
            }
        }

        public RigReferenceCollection() { }

        public RigReferenceCollection(RigManager rigManager) {
            RigManager = rigManager;
            RigGrips = rigManager.physicsRig.GetComponentsInChildren<Grip>(true);
            RigSlots = rigManager.GetComponentsInChildren<InventorySlotReceiver>(true);

            LeftHand = rigManager.physicsRig.m_handLf.GetComponent<Hand>();
            RightHand = rigManager.physicsRig.m_handRt.GetComponent<Hand>();

            LeftController = rigManager.openControllerRig.leftController;
            RightController = rigManager.openControllerRig.rightController;

            LeftSnatchGrip = null;
            RightSnatchGrip = null;
        }
    }

    public static class RigData
    {
        public static RigReferenceCollection RigReferences { get; private set; } = new RigReferenceCollection();

        public static string RigScene { get; private set; }
        public static string RigAvatarId { get; private set; } = AvatarWarehouseUtilities.INVALID_AVATAR_BARCODE;

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

            if (NetworkInfo.HasServer) {
                EnableRagdollOnDeath();
            }

            RigSpawn = rigObject.transform.position;
            RigSpawnRot = rigObject.transform.rotation;
        }

        public static void EnableRagdollOnDeath() {
            if (!RigReferences.RigManager.IsNOC()) {
                var health = RigReferences.RigManager.health;

                if (health.healthMode != Health.HealthMode.Invincible)
                    health._testRagdollOnDeath = true;
            }
        }

        public static void OnRigRescale() {
            // Send body vitals to network
            if (NetworkInfo.HasServer) {
                using (FusionWriter writer = FusionWriter.Create()) {
                    using (PlayerRepVitalsData data = PlayerRepVitalsData.Create(PlayerIdManager.LocalSmallId, RigReferences.RigManager.bodyVitals)) {
                        writer.Write(data);

                        using (var message = FusionMessage.Create(NativeMessageTag.PlayerRepVitals, writer)) {
                            MessageSender.BroadcastMessageExceptSelf(NetworkChannel.Reliable, message);
                        }
                    }
                }
            }
        }

        public static void OnRigHandUpdate(Hand hand) {
            if (!hand || !hand.m_CurrentAttachedGO || !hand.joint)
                return;

            var grip = Grip.Cache.Get(hand.m_CurrentAttachedGO);

            if (NetworkInfo.HasServer)
            {
                using (FusionWriter writer = FusionWriter.Create())
                {
                    using (PlayerRepAnchorData data = PlayerRepAnchorData.Create(PlayerIdManager.LocalSmallId, new SerializedGripAnchor(hand, grip)))
                    {
                        writer.Write(data);

                        using (var message = FusionMessage.Create(NativeMessageTag.PlayerRepAnchors, writer))
                        {
                            MessageSender.BroadcastMessageExceptSelf(NetworkChannel.Unreliable, message);
                        }
                    }
                }
            }
        }

        public static void OnRigUpdate() {
            if (RigReferences.RigManager) {
                var barcode = GetAvatarBarcode();
                if (barcode != RigAvatarId) {
                    // Send switch message to notify the server
                    if (NetworkInfo.HasServer) {
                        using (FusionWriter writer = FusionWriter.Create()) {
                            using (PlayerRepAvatarData data = PlayerRepAvatarData.Create(PlayerIdManager.LocalSmallId, barcode)) {
                                writer.Write(data);

                                using (var message = FusionMessage.Create(NativeMessageTag.PlayerRepAvatar, writer)) {
                                    MessageSender.BroadcastMessageExceptSelf(NetworkChannel.Reliable, message);
                                }
                            }
                        }
                    }
                }
                RigAvatarId = barcode;

                if (RigReferences.LeftHand)
                    OnRigHandUpdate(RigReferences.LeftHand);

                if (RigReferences.RightHand)
                    OnRigHandUpdate(RigReferences.RightHand);
            }
        }

        public static string GetAvatarBarcode() {
            if (RigReferences.RigManager)
                return RigReferences.RigManager.AvatarCrate.Barcode;
            return AvatarWarehouseUtilities.INVALID_AVATAR_BARCODE;
        }
    }
}
