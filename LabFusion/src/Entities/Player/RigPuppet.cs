using System.Collections;

using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Interaction;
using Il2CppSLZ.Marrow.Pool;
using Il2CppSLZ.Marrow.VFX;

using LabFusion.Extensions;
using LabFusion.Representation;
using LabFusion.Utilities;

using MelonLoader;

using UnityEngine;

namespace LabFusion.Entities;

public class RigPuppet
{
    private RigRefs _selfReferences = null;

    public bool HasPuppet => _selfReferences != null && _selfReferences.IsValid;

    public void CreatePuppet(Action<RigManager> onPuppetCreated = null)
    {
        // Destroy any extra rigmanager if it exists
        DestroyPuppet();

        // Create the puppet
        PlayerRepUtilities.CreateNewRig((rig) =>
        {
            OnPuppetCreated(rig, onPuppetCreated);
        });
    }

    private void OnPuppetCreated(RigManager rig, Action<RigManager> onPuppetCreated = null)
    {
        // Add poolee to the PhysicsRig for spawn/despawn VFX
        rig.physicsRig.gameObject.AddComponent<Poolee>();

        // Swap the open controllers for generic controllers
        // Left hand
        var leftHaptor = rig.ControllerRig.leftController.haptor;
        rig.ControllerRig.leftController = rig.ControllerRig.leftController.gameObject.AddComponent<BaseController>();
        rig.ControllerRig.leftController.contRig = rig.ControllerRig;
        leftHaptor.device_Controller = rig.ControllerRig.leftController;
        rig.ControllerRig.leftController.handedness = Handedness.LEFT;

        // Right hand
        var rightHaptor = rig.ControllerRig.rightController.haptor;
        rig.ControllerRig.rightController = rig.ControllerRig.rightController.gameObject.AddComponent<BaseController>();
        rig.ControllerRig.rightController.contRig = rig.ControllerRig;
        rightHaptor.device_Controller = rig.ControllerRig.rightController;
        rig.ControllerRig.rightController.handedness = Handedness.RIGHT;

        // Get references
        _selfReferences = new RigRefs(rig);

        // Shrink holster hitboxes for easier grabbing
        foreach (var slot in _selfReferences.RigSlots)
        {
            foreach (var box in slot.GetComponentsInChildren<BoxCollider>())
            {
                // Only affect trigger colliders just incase
                if (box.isTrigger)
                    box.size *= 0.4f;
            }
        }

        // Invoke callback
        onPuppetCreated?.Invoke(rig);

        // Play spawn VFX
        MelonCoroutines.Start(WaitAndCallSpawnEffect(rig.physicsRig.marrowEntity));
    }

    private static IEnumerator WaitAndCallSpawnEffect(MarrowEntity marrowEntity)
    {
        float elapsed = 0f;

        while (elapsed < 0.5f)
        {
            elapsed += TimeUtilities.DeltaTime;
            yield return null;
        }

        if (marrowEntity == null)
        {
            yield break;
        }

        SpawnEffects.CallSpawnEffect(marrowEntity);
    }

    public void DestroyPuppet()
    {
        if (!HasPuppet)
        {
            return;
        }

        var marrowEntity = _selfReferences.RigManager.physicsRig.marrowEntity;

        SpawnEffects.CallDespawnEffect(marrowEntity);

        _selfReferences.LeftHand.TryDetach();
        _selfReferences.RightHand.TryDetach();

        GameObject.Destroy(_selfReferences.RigManager.gameObject);
    }
}
