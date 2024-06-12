using Il2CppSLZ.Rig;
using Il2CppSLZ.Interaction;

using BoneLib;

using UnityEngine;

using LabFusion.Utilities;
using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.Extensions;
using LabFusion.Senders;

using Il2CppSLZ.Bonelab;
using Il2CppSLZ.Marrow.AI;
using Il2CppSLZ.Marrow.Interaction;

using CommonBarcodes = LabFusion.Utilities.CommonBarcodes;

namespace LabFusion.Data
{
    /// <summary>
    /// A collection of basic rig information for use across PlayerReps and the Main RigManager.
    /// </summary>
    public class RigReferenceCollection
    {
        public bool IsValid { get; private set; } = false;

        public RigManager RigManager { get; private set; }
        public ControllerRig ControllerRig { get; private set; }

        public Player_Health Health { get; private set; }

        public Grip[] RigGrips { get; private set; }
        public Rigidbody[] RigRigidbodies { get; private set; }

        public InventorySlotReceiver[] RigSlots { get; private set; }

        public Hand LeftHand { get; private set; }
        public Hand RightHand { get; private set; }

        public TriggerRefProxy Proxy { get; private set; }

        public BaseController LeftController { get; private set; }
        public BaseController RightController { get; private set; }

        public Transform Head { get; private set; }

        public void OnDestroy()
        {
            IsValid = false;
            RigRigidbodies = null;
        }

        public byte? GetIndex(Grip grip, bool isAvatarGrip = false)
        {
            var gripArray = RigGrips;

            if (isAvatarGrip)
                gripArray = GetAvatarGrips();

            for (byte i = 0; i < gripArray.Length; i++)
            {
                if (gripArray[i] == grip)
                    return i;
            }
            return null;
        }

        public Grip GetGrip(byte index, bool isAvatarGrip = false)
        {
            var gripArray = RigGrips;

            if (isAvatarGrip)
                gripArray = GetAvatarGrips();

            if (gripArray != null && gripArray.Length > index)
                return gripArray[index];
            return null;
        }

        internal Grip[] GetAvatarGrips()
        {
            return RigManager._avatar.GetComponentsInChildren<Grip>();
        }

        internal InventorySlotReceiver[] GetAvatarSlots()
        {
            return RigManager._avatar.GetComponentsInChildren<InventorySlotReceiver>();
        }

        // Rigidbody order likes to randomly change on players
        // So we have to disgustingly update it every index call
        internal void GetRigidbodies()
        {
            if (!IsValid)
                return;

            RigRigidbodies = RigManager.physicsRig.GetComponentsInChildren<Rigidbody>(true);
        }

        public byte? GetIndex(Rigidbody rb)
        {
            GetRigidbodies();

            for (byte i = 0; i < RigRigidbodies.Length; i++)
            {
                if (RigRigidbodies[i] == rb)
                    return i;
            }
            return null;
        }

        public Rigidbody GetRigidbody(byte index)
        {
            GetRigidbodies();

            if (RigRigidbodies != null && RigRigidbodies.Length > index)
                return RigRigidbodies[index];
            return null;
        }

        public byte? GetIndex(InventorySlotReceiver slot, bool isAvatarSlot = false)
        {
            var slotArray = RigSlots;

            if (isAvatarSlot)
                slotArray = GetAvatarSlots();

            for (byte i = 0; i < slotArray.Length; i++)
            {
                if (slotArray[i] == slot)
                    return i;
            }
            return null;
        }

        public InventorySlotReceiver GetSlot(byte index, bool isAvatarSlot = false)
        {
            var slotArray = RigSlots;

            if (isAvatarSlot)
                slotArray = GetAvatarSlots();

            if (slotArray != null && slotArray.Length > index)
                return slotArray[index];
            return null;
        }

        public Hand GetHand(Handedness handedness)
        {
            return handedness switch
            {
                Handedness.LEFT => LeftHand,
                Handedness.RIGHT => RightHand,
                _ => null,
            };
        }

        public void DisableInteraction()
        {
            if (RigGrips == null)
                return;

            foreach (var grip in RigGrips)
            {
                foreach (var hand in grip.attachedHands.ToArray())
                {
                    if (hand.manager.IsSelf())
                        grip.TryDetach(hand);
                }

                grip.DisableInteraction();
            }

            DelayUtilities.Delay(Internal_DelayedEnableInteraction, 300);
        }

        private void Internal_DelayedEnableInteraction()
        {
            if (RigGrips == null)
                return;

            foreach (var grip in RigGrips)
            {
                grip.EnableInteraction();
            }
        }

        public RigReferenceCollection() { }

        public RigReferenceCollection(RigManager rigManager)
        {
            // Get the rig manager and hook when its destroyed
            RigManager = rigManager;
            RigRigidbodies = null;
            IsValid = true;

            var lifeCycle = rigManager.gameObject.AddComponent<RigLifeCycleEvents>();
            lifeCycle.Collection = this;

            // Assign values
            ControllerRig = rigManager.ControllerRig;

            Health = RigManager.health.Cast<Player_Health>();

            RigGrips = rigManager.physicsRig.GetComponentsInChildren<Grip>(true);

            RigSlots = rigManager.GetComponentsInChildren<InventorySlotReceiver>(true);

            LeftHand = rigManager.physicsRig.m_handLf.GetComponent<Hand>();
            RightHand = rigManager.physicsRig.m_handRt.GetComponent<Hand>();

            Proxy = rigManager.GetComponentInChildren<TriggerRefProxy>(true);

            LeftController = rigManager.ControllerRig.leftController;
            RightController = rigManager.ControllerRig.rightController;

            Head = RigManager.physicsRig.m_head;
        }
    }

    public static class RigData
    {
        public static RigReferenceCollection RigReferences { get; private set; } = new RigReferenceCollection();
        public static bool HasPlayer => RigReferences.IsValid;

        public static string RigAvatarId { get; internal set; } = CommonBarcodes.INVALID_AVATAR_BARCODE;
        public static SerializedAvatarStats RigAvatarStats { get; internal set; } = null;

        public static Vector3 RigSpawn { get; private set; }
        public static Quaternion RigSpawnRot { get; private set; }

        private static bool _hookedPauseEvent = false;

        private static void OnJump()
        {
            if (NetworkInfo.HasServer)
            {
                PlayerSender.SendPlayerAction(PlayerActionType.JUMP);
            }
        }

        private static void OnPauseStateChanged(bool paused)
        {
            if (HasPlayer && !paused)
            {
                RigReferences.RigManager.GetComponentInChildren<BodyVitals>().CalibratePlayerBodyScale();
            }
        }

        public static void OnCacheRigInfo()
        {
            var manager = Player.rigManager;

            if (!manager)
            {
                return;
            }

            if (!_hookedPauseEvent)
            {
                OpenControllerRig.OnPauseStateChange += (Il2CppSystem.Action<bool>)OnPauseStateChanged;
            }

            // Store spawn values
            RigSpawn = manager.transform.position;
            RigSpawnRot = manager.transform.rotation;

            // Store the references
            RigReferences = new RigReferenceCollection(manager);
            RigReferences.RigManager.GetComponentInChildren<BodyVitals>().rescaleEvent += (BodyVitals.RescaleUI)OnSendVitals;
            RigReferences.RigManager.remapHeptaRig.onPlayerJump += (Il2CppSystem.Action)OnJump;

            // Notify hooks
            MultiplayerHooking.Internal_OnLocalPlayerCreated(manager);

            // Update avatar
            if (manager._avatar != null)
                FusionPlayer.Internal_OnAvatarChanged(manager, manager._avatar, manager.AvatarCrate.Barcode);
        }

        public static void OnSendVitals()
        {
            // Send body vitals to network
            if (NetworkInfo.HasServer)
            {
                using FusionWriter writer = FusionWriter.Create(PlayerRepVitalsData.Size);
                var data = PlayerRepVitalsData.Create(PlayerIdManager.LocalSmallId, RigReferences.RigManager.GetComponentInChildren<BodyVitals>());
                writer.Write(data);

                using var message = FusionMessage.Create(NativeMessageTag.PlayerRepVitals, writer);
                MessageSender.BroadcastMessageExceptSelf(NetworkChannel.Reliable, message);
            }
        }

        public static string GetAvatarBarcode()
        {
            var rm = RigReferences.RigManager;

            if (rm)
                return rm.AvatarCrate.Barcode;
            return CommonBarcodes.INVALID_AVATAR_BARCODE;
        }
    }
}
