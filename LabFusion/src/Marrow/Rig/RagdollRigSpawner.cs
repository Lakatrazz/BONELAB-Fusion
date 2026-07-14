using Il2CppSLZ.Marrow;

using LabFusion.Utilities;

using UnityEngine;

namespace LabFusion.Marrow.Rig;

public static class RagdollRigSpawner
{
    public const string RagdollRigName = "[RigManager (Ragdoll)]";

    public static void SpawnRagdollRig(RigManager referenceRig, Action<RigManager> spawnCallback)
    {
        var transform = referenceRig.transform;
        var position = transform.position;
        var rotation = transform.rotation;

        SpawnRagdollRig(OnRagdollRigSpawned);

        void OnRagdollRigSpawned(RigManager rigManager)
        {
            OnApplyReferenceRig(referenceRig, rigManager);

            spawnCallback?.Invoke(rigManager);
        }
    }

    public static void SpawnRagdollRig(Action<RigManager> spawnCallback)
    {
        DummyRigCreator.CreateDummyRig(new DummyRigCreator.DummyRigCreationInfo()
        {
            Position = Vector3.zero,
            Rotation = Quaternion.identity,
            SpawnCallback = OnDummyRigCreated,
        });

        void OnDummyRigCreated(RigManager rigManager)
        {
            OnRagdollRigCreated(rigManager);

            spawnCallback?.Invoke(rigManager);
        }
    }

    private static void OnApplyReferenceRig(RigManager referenceRig, RigManager ragdollRig)
    {
        // Copy the avatar
        ragdollRig.SwapAvatarCrate(referenceRig.AvatarCrate.Barcode, false, (Action<bool>)OnAvatarLoaded);

        // Copy the hand poses
        var referenceControllerRig = referenceRig.ControllerRig;
        var ragdollControllerRig = ragdollRig.ControllerRig;

        OnApplyReferenceToController(referenceControllerRig.leftController, ragdollControllerRig.leftController);
        OnApplyReferenceToController(referenceControllerRig.rightController, ragdollControllerRig.rightController);

        // Copy the marrow entity's pose
        // Do it in two frames after the rig is ragdolled to prevent offset issues
        DelayUtilities.InvokeDelayed(LateCopyPose, 2);

        void LateCopyPose()
        {
            var referenceMarrowEntity = referenceRig.physicsRig.marrowEntity;
            var ragdollMarrowEntity = ragdollRig.physicsRig.marrowEntity;

            for (var i = 0; i < referenceMarrowEntity.Bodies.Count; i++)
            {
                var referenceMarrowBody = referenceMarrowEntity.Bodies[i];
                var referenceRigidbody = referenceMarrowBody._rigidbody;

                if (!referenceRigidbody)
                {
                    continue;
                }

                var ragdollMarrowBody = ragdollMarrowEntity.Bodies[i];
                var ragdollRigidbody = ragdollMarrowBody._rigidbody;

                if (!ragdollRigidbody)
                {
                    continue;
                }

                ragdollRigidbody.transform.SetPositionAndRotation(referenceRigidbody.transform.position, referenceRigidbody.transform.rotation);

                ragdollRigidbody.velocity = referenceRigidbody.velocity;
                ragdollRigidbody.angularVelocity = referenceRigidbody.angularVelocity;
            }
        }

        void OnAvatarLoaded(bool succeeded)
        {
            OnRagdollAvatarLoaded(ragdollRig);

            CopyHitEffects();
        }

        void CopyHitEffects()
        {
            try
            {
                var referenceHealth = referenceRig.health;
                var ragdollHealth = ragdollRig.health;

                foreach (var hitPoint in referenceHealth.HitPoint)
                {
                    ragdollHealth.AddToHitArray(hitPoint);
                }
            }
            catch { }
        }
    }

    private static void OnApplyReferenceToController(BaseController referenceController, BaseController ragdollController)
    {
        ragdollController._processedIndex = referenceController._processedIndex;
        ragdollController._processedMiddle = referenceController._processedMiddle;
        ragdollController._processedRing = referenceController._processedRing;
        ragdollController._processedPinky = referenceController._processedPinky;
        ragdollController._processedThumb = referenceController._processedThumb;
    }

    private static void OnRagdollRigCreated(RigManager rigManager)
    {
        rigManager.gameObject.name = RagdollRigName;

        // Disable rigs that aren't needed for a ragdoll
        DisableUnnecessaryRigs(rigManager);

        // Force the rig into a ragdolled state
        // Do it next frame to prevent rig issues
        DelayUtilities.InvokeNextFrame(LateRagdoll);

        // Remove rig forces to prevent issues during the frame delay
        var physicsRig = rigManager.physicsRig;

        physicsRig._pelvisForceMult = 0f;
        physicsRig._pelvisForceInternalMult = 0f;

        // Remove slots
        var inventory = rigManager.inventory;

        foreach (var slot in inventory.bodySlots)
        {
            slot.gameObject.SetActive(false);
        }

        foreach (var slot in inventory.specialItems)
        {
            slot.gameObject.SetActive(false);
        }

        // Add general rig additions
        RigAdditions.ApplyRigAdditions(rigManager);

        void LateRagdoll()
        {
            var physicsRig = rigManager.physicsRig;

            physicsRig.RagdollRig();
            physicsRig.ShutdownRig();
        }
    }

    private static void OnRagdollAvatarLoaded(RigManager rigManager)
    {
        // Disable animators so that eye blinking/idle behavior stops when dead
        var avatar = rigManager.avatar;

        var animators = avatar.GetComponentsInChildren<Animator>();

        foreach (var animator in animators)
        {
            animator.enabled = false;
        }
    }

    private static void DisableUnnecessaryRigs(RigManager rigManager)
    {
        foreach (var remapRig in rigManager.remapRigs)
        {
            remapRig.gameObject.SetActive(false);
        }

        rigManager.ControllerRig.gameObject.SetActive(false);
    }
}
