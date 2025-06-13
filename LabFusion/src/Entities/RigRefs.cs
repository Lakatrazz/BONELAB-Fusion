using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.AI;
using Il2CppSLZ.Marrow.Interaction;

using LabFusion.Extensions;
using LabFusion.MonoBehaviours;
using LabFusion.Utilities;

using UnityEngine;

namespace LabFusion.Entities;

public class RigRefs
{
    public bool IsValid { get; private set; } = false;

    public RigManager RigManager { get; private set; }
    public ControllerRig ControllerRig { get; private set; }

    public Player_Health Health { get; private set; }

    public Grip[] RigGrips { get; private set; }

    public InventorySlotReceiver[] RigSlots { get; private set; }

    public Hand LeftHand { get; private set; }
    public Hand RightHand { get; private set; }

    public TriggerRefProxy Proxy { get; private set; }

    public Transform Head { get; private set; }
    public Transform Mouth { get; private set; }

    public Transform Headset { get; private set; }

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
                if (hand.manager.IsLocalPlayer())
                    grip.TryDetach(hand);
            }

            grip.DisableInteraction();
        }

        DelayUtilities.InvokeDelayed(Internal_DelayedEnableInteraction, 300);
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

    public RigRefs() { }

    public RigRefs(RigManager rigManager)
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

        LeftHand = rigManager.physicsRig.m_handLf.GetComponent<Hand>();
        RightHand = rigManager.physicsRig.m_handRt.GetComponent<Hand>();

        Proxy = rigManager.GetComponentInChildren<TriggerRefProxy>(true);

        Head = RigManager.physicsRig.m_head;
        Mouth = RigManager.physicsRig.headSfx.mouthSrc.transform;

        var openControllerRig = ControllerRig.TryCast<OpenControllerRig>();

        if (openControllerRig != null)
        {
            Headset = openControllerRig.headset;
        }
        else
        {
            Headset = ControllerRig.m_head;
        }
    }
}