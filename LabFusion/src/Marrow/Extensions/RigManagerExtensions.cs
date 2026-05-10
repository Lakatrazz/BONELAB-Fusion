using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Interaction;
using Il2CppSLZ.Marrow.Utilities;
using Il2CppSLZ.Marrow.Warehouse;

using LabFusion.Entities;
using LabFusion.Marrow.Extensions;

using MelonLoader;

using System.Collections;

using UnityEngine;

using Avatar = Il2CppSLZ.VRMK.Avatar;

namespace LabFusion.Extensions;

public static class RigManagerExtensions
{
    private struct TempBody
    {
        public Rigidbody Rigidbody;
        public Transform Transform;
        public Vector3 Position;
        public Vector3 Velocity;
    }

    /// <summary>
    /// Teleports a RigManager seamlessly by using a position and velocity offset without resetting its body positions.
    /// </summary>
    /// <param name="rigManager">The RigManager to teleport.</param>
    /// <param name="positionOffset">The offset to apply to all rigidbody positions.</param>
    /// <param name="velocityOffset">The offset to apply to all rigidbody velocities.</param>
    public static void TeleportWithOffset(this RigManager rigManager, Vector3 positionOffset, Vector3 velocityOffset)
    {
        var controllerRig = rigManager.ControllerRig;
        var physicsRig = rigManager.physicsRig;
        var remapRigs = rigManager.remapRigs;

        var marrowEntity = physicsRig.marrowEntity;

        // Gets the teleported positions and velocities for all of the RigManager's rigidbodies
        var tempBodies = new List<TempBody>();
        foreach (var marrowBody in marrowEntity.Bodies)
        {
            if (!marrowBody.HasRigidbody)
            {
                continue;
            }

            var rigidbody = marrowBody._rigidbody;
            var transform = marrowBody.transform;

            tempBodies.Add(new TempBody()
            {
                Rigidbody = rigidbody,
                Transform = transform,
                Position = transform.position + positionOffset,
                Velocity = rigidbody.velocity + velocityOffset,
            });
        }

        // Teleporting all of the rigs that need it
        var displaceTransform = SimpleTransform.Create(positionOffset, Quaternion.identity);

        controllerRig.Teleport(displaceTransform, false);

        foreach (var rig in remapRigs)
        {
            rig.Teleport(displaceTransform, false);
        }

        physicsRig.Teleport(displaceTransform, false);

        // Now, reapply the teleported positions and velocities for the player's rigidbodies
        // This makes it more stable than the regular PhysicsRig teleport
        foreach (var tempBody in tempBodies)
        {
            tempBody.Transform.position = tempBody.Position;
            tempBody.Rigidbody.velocity = tempBody.Velocity;
        }
    }

    public static void TeleportToPosition(this RigManager rigManager, Vector3 position, bool resetVelocity = true)
    {
        var physicsRig = rigManager.physicsRig;
        var marrowEntity = physicsRig.marrowEntity;

        marrowEntity.ResetPose(resetVelocity);
        physicsRig.centerOfPressure.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

        TeleportRig(physicsRig, position, resetVelocity);

        physicsRig.ResetHands(Handedness.BOTH);

        foreach (var rig in rigManager.remapRigs)
        {
            TeleportRig(rig, position, resetVelocity);
        }
    }

    public static void TeleportToPosition(this RigManager rigManager, Vector3 position, Vector3 forward, bool resetVelocity = true)
    {
        var remapRig = rigManager.remapHeptaRig;
        var physicsRig = rigManager.physicsRig;
        var marrowEntity = physicsRig.marrowEntity;

        marrowEntity.ResetPose(resetVelocity);
        physicsRig.centerOfPressure.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

        TeleportRig(physicsRig, position, forward, resetVelocity);

        physicsRig.ResetHands(Handedness.BOTH);

        remapRig.SetTwist(Vector3.SignedAngle(remapRig.centerOfPressure.forward, forward, Vector3.up));

        foreach (var rig in rigManager.remapRigs)
        {
            TeleportRig(rig, position, forward, resetVelocity);
        }
    }

    private static void TeleportRig(Rig rig, Vector3 position, bool resetVelocity)
    {
        var displace = SimpleTransform.Create(position - rig.centerOfPressure.position, Quaternion.identity);

        rig.Teleport(displace, resetVelocity);
    }

    private static void TeleportRig(Rig rig, Vector3 position, Vector3 forward, bool resetVelocity)
    {
        var displace = SimpleTransform.Create(position - rig.centerOfPressure.position, Quaternion.FromToRotation(rig.centerOfPressure.forward, forward));

        rig.Teleport(displace, resetVelocity);
    }

    public static void SwapAvatarCrate(this RigRefs references, string barcode, Action<bool> callback = null, Action<string, GameObject> preSwapAvatar = null)
    {
        AvatarCrateReference crateRef = new(barcode);
        var crate = crateRef.Crate;

        if (crate == null)
        {
            callback?.Invoke(false);
        }
        else
        {
            MelonCoroutines.Start(CoWaitAndSwapAvatarRoutine(references, crate, callback, preSwapAvatar));
        }
    }

    private static IEnumerator CoWaitAndSwapAvatarRoutine(RigRefs references, AvatarCrate crate, Action<bool> callback = null, Action<string, GameObject> preSwapAvatar = null)
    {
        bool loaded = false;
        GameObject avatar = null;

        crate.LoadAsset((Il2CppSystem.Action<GameObject>)((go) =>
        {
            loaded = true;
            avatar = go;
        }));

        while (!loaded)
            yield return null;

        if (!references.IsValid)
            yield break;

        if (avatar == null)
        {
            callback?.Invoke(false);
        }
        else
        {
            var rm = references.RigManager;
            GameObject instance = GameObject.Instantiate(avatar);
            instance.SetActive(false);
            instance.name = avatar.name;

            preSwapAvatar?.Invoke(crate.Barcode.ID, instance);

            instance.transform.parent = references.RigManager.transform;
            instance.transform.SetLocalPositionAndRotation(Vector3Extensions.zero, QuaternionExtensions.identity);

            var avatarComponent = instance.GetComponentInParent<Avatar>(true);
            rm.SwapAvatar(avatarComponent);

            while (references.IsValid && rm.avatar != avatarComponent)
                yield return null;

            if (!references.IsValid)
                yield break;

            rm._avatarCrate = new AvatarCrateReference(crate.Barcode);
            rm.onAvatarSwapped?.Invoke();
            rm.onAvatarSwapped2?.Invoke(crate.Barcode);
            callback?.Invoke(true);
        }
    }

}