using UnityEngine;

using LabFusion.Utilities;
using LabFusion.Network;
using LabFusion.Player;
using LabFusion.Extensions;
using LabFusion.Senders;
using LabFusion.MonoBehaviours;

using Il2CppSLZ.Bonelab;
using Il2CppSLZ.Marrow.AI;
using Il2CppSLZ.Marrow.Interaction;
using Il2CppSLZ.Marrow;

using CommonBarcodes = LabFusion.Utilities.CommonBarcodes;

namespace LabFusion.Data;

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

    public InventorySlotReceiver[] RigSlots { get; private set; }

    public InventoryAmmoReceiver AmmoReceiver { get; private set; }

    public Hand LeftHand { get; private set; }
    public Hand RightHand { get; private set; }

    public TriggerRefProxy Proxy { get; private set; }

    public Transform Head { get; private set; }

    private Action _onDestroyCallback = null;

    public void HookOnDestroy(Action callback)
    {
        if (IsValid)
        {
            _onDestroyCallback += callback;
        }
        else
        {
            _onDestroyCallback?.Invoke();
        }
    }

    public void OnDestroy()
    {
        IsValid = false;

        _onDestroyCallback?.Invoke();
    }

    internal InventorySlotReceiver[] GetAvatarSlots()
    {
        return RigManager._avatar.GetComponentsInChildren<InventorySlotReceiver>();
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
        IsValid = true;

        var destroySensor = rigManager.gameObject.AddComponent<DestroySensor>();
        destroySensor.Hook(OnDestroy);

        // Assign values
        ControllerRig = rigManager.ControllerRig;

        Health = RigManager.health.Cast<Player_Health>();

        RigGrips = rigManager.physicsRig.GetComponentsInChildren<Grip>(true);

        RigSlots = rigManager.GetComponentsInChildren<InventorySlotReceiver>(true);

        AmmoReceiver = rigManager.GetComponentInChildren<InventoryAmmoReceiver>(true);

        LeftHand = rigManager.physicsRig.m_handLf.GetComponent<Hand>();
        RightHand = rigManager.physicsRig.m_handRt.GetComponent<Hand>();

        Proxy = rigManager.GetComponentInChildren<TriggerRefProxy>(true);

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

    private static void OnJump()
    {
        if (NetworkInfo.HasServer)
        {
            PlayerSender.SendPlayerAction(PlayerActionType.JUMP);
        }
    }

    public static void OnCacheRigInfo()
    {
        var manager = PlayerRefs.Instance.PlayerRigManager;

        if (manager == null)
        {
            FusionLogger.Error("Failed to find the Player's RigManager!");
            return;
        }

        // Store spawn values
        RigSpawn = manager.transform.position;
        RigSpawnRot = manager.transform.rotation;

        // Store the references
        RigReferences = new RigReferenceCollection(manager);
        RigReferences.RigManager.remapHeptaRig.onPlayerJump += (Il2CppSystem.Action)OnJump;

        PlayerRefs.Instance.PlayerBodyVitals.rescaleEvent += (BodyVitals.RescaleUI)OnSendVitals;

        // Notify hooks
        LocalPlayer.OnLocalRigCreated?.InvokeSafe(manager, "executing OnLocalRigCreated hook");

        // Update avatar
        if (manager._avatar != null)
        {
            FusionPlayer.Internal_OnAvatarChanged(manager, manager._avatar, manager.AvatarCrate.Barcode.ID);
        }
    }

    public static void OnSendVitals()
    {
        // Make sure we have a server
        if (!NetworkInfo.HasServer)
        {
            return;
        }

        // Send body vitals to network
        using FusionWriter writer = FusionWriter.Create(PlayerRepVitalsData.Size);
        var data = PlayerRepVitalsData.Create(PlayerIdManager.LocalSmallId, PlayerRefs.Instance.PlayerBodyVitals);
        writer.Write(data);

        using var message = FusionMessage.Create(NativeMessageTag.PlayerRepVitals, writer);
        MessageSender.BroadcastMessageExceptSelf(NetworkChannel.Reliable, message);
    }

    public static string GetAvatarBarcode()
    {
        var rm = RigReferences.RigManager;

        if (rm)
            return rm.AvatarCrate.Barcode.ID;
        return CommonBarcodes.INVALID_AVATAR_BARCODE;
    }
}