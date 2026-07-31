using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Interaction;
using Il2CppSLZ.Marrow.Utilities;
using Il2CppSLZ.Marrow.Warehouse;

using LabFusion.Entities;
using LabFusion.Extensions;

using MelonLoader;

using System.Collections;

using UnityEngine;

using Avatar = Il2CppSLZ.VRMK.Avatar;

namespace LabFusion.Marrow.Extensions;

using Rig = Il2CppSLZ.Marrow.Rig;

public static class RigManagerExtensions
{
    public struct AvatarSwitchInfo
    {
        public string Barcode { get; set; }

        public Action<string, Avatar> BeforeSwapAvatarCallback { get; set; }

        public Action<bool> CompletedCallback { get; set; }
    }

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
        TeleportToPosition(rigManager, position, resetVelocity);

        var remapRig = rigManager.remapHeptaRig;

        remapRig.SetTwist(Vector3.SignedAngle(remapRig.centerOfPressure.forward, forward, Vector3.up));
    }

    private static void TeleportRig(Rig rig, Vector3 position, bool resetVelocity)
    {
        var displace = SimpleTransform.Create(position - rig.centerOfPressure.position, Quaternion.identity);

        rig.Teleport(displace, resetVelocity);
    }

    public static void SwitchAvatarWithCallbacks(this RigManager rigManager, AvatarSwitchInfo info)
    {
        var crateReference = new AvatarCrateReference(info.Barcode);
        var crate = crateReference.Crate;

        if (crate == null)
        {
            info.CompletedCallback?.Invoke(false);
            return;
        }

        crate.LoadAsset((Il2CppSystem.Action<GameObject>)(asset =>
        {
            MelonCoroutines.Start(CoSwitchAvatarWithCallbacks(rigManager, info, crate, asset));
        }));
    }

    private static IEnumerator CoSwitchAvatarWithCallbacks(RigManager rigManager, AvatarSwitchInfo info, AvatarCrate crate, GameObject asset)
    {
        if (rigManager == null)
        {
            FailAvatarSwap();
            yield break;
        }

        if (asset == null)
        {
            FailAvatarSwap();
            yield break;
        }

        var avatarParent = rigManager.transform;

        var avatarInstance = GameObject.Instantiate(asset, avatarParent);
        avatarInstance.name = asset.name;
        avatarInstance.SetActive(true);

        var avatarTransform = avatarInstance.transform;
        avatarTransform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

        var avatar = avatarInstance.GetComponent<Avatar>();

        if (avatar == null)
        {
            GameObject.Destroy(avatarInstance);

            FailAvatarSwap();
            yield break;
        }

        info.BeforeSwapAvatarCallback?.Invoke(info.Barcode, avatar);

        avatarInstance.SetActive(false);

        rigManager.SwapAvatar(avatar);

        while (rigManager != null && rigManager._avatarDirty)
        {
            yield return null;
        }

        if (rigManager == null)
        {
            FailAvatarSwap();
            yield break;
        }

        var crateBarcode = crate.Barcode;

        rigManager._avatarCrate = new AvatarCrateReference(crateBarcode);
        rigManager.onAvatarSwapped?.Invoke();
        rigManager.onAvatarSwapped2?.Invoke(crateBarcode);

        info.CompletedCallback?.Invoke(true);

        void FailAvatarSwap()
        {
            info.CompletedCallback?.Invoke(false);
        }
    }
}