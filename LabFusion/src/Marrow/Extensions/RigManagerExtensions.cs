using MelonLoader;

using Il2CppSLZ.Marrow.Warehouse;
using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Utilities;
using Il2CppSLZ.Marrow.Interaction;

using System.Collections;

using UnityEngine;

using Avatar = Il2CppSLZ.VRMK.Avatar;

using LabFusion.Entities;
using LabFusion.Marrow.Extensions;

namespace LabFusion.Extensions;

public static class RigManagerExtensions
{
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