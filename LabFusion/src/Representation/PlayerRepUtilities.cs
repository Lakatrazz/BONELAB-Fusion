using UnityEngine;
using UnityEngine.Rendering.Universal;

using LabFusion.Utilities;
using LabFusion.Data;
using LabFusion.Marrow;
using LabFusion.Entities;
using LabFusion.MonoBehaviours;

using Il2Cpp;

using Il2CppSLZ.Bonelab;
using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Interaction;
using Il2CppSLZ.Marrow.Warehouse;

using Il2CppInterop.Runtime.InteropTypes.Arrays;

namespace LabFusion.Representation;

public static class PlayerRepUtilities
{
    // This should never change, incase other mods rely on it.
    public const string PlayerRepName = "[RigManager (Networked)]";

    public static bool TryGetReferences(byte smallId, out RigRefs references)
    {
        references = null;

        if (NetworkPlayerManager.TryGetPlayer(smallId, out var player))
        {
            references = player.RigRefs;
            return true;
        }

        return false;
    }

    public static void CreateNewRig(Action<RigManager> onRigCreated)
    {
        if (MarrowSettings.RuntimeInstance == null)
        {
            return;
        }

        var crate = MarrowSettings.RuntimeInstance.DefaultPlayerRig.Crate;
        if (crate == null)
        {
            return;
        }

        crate.LoadAsset((Action<GameObject>)((go) => Internal_OnLoadPlayer(go, onRigCreated)));
    }

    private static void Internal_OnLoadPlayer(GameObject asset, Action<RigManager> onRigCreated)
    {
        // Create a temporary parent that is disabled
        GameObject tempParent = new();
        tempParent.SetActive(false);

        var rigAsset = asset.GetComponentInChildren<RigManager>().gameObject;

        var go = GameObject.Instantiate(rigAsset, tempParent.transform);
        go.name = PlayerRepName;
        go.SetActive(false);

        if (RigData.Refs.RigManager)
        {
            go.transform.position = RigData.RigSpawn;
            go.transform.rotation = RigData.RigSpawnRot;
        }

        var rigManager = go.GetComponent<RigManager>();
        Internal_SetupPlayerRep(rigManager);

        go.transform.parent = null;
        GameObject.Destroy(tempParent);

        go.SetActive(true);

        onRigCreated?.Invoke(rigManager);
    }

    private static void Internal_SetupPlayerRep(RigManager rigManager)
    {
        // IMPORTANT NOTICE!
        // When destroying stuff on this player, make sure to use DestroyImmediate!
        // Destroy is "safe", and waits for the next frame to destroy the object.
        // However, this means methods on that object will be called, even if we want it to have never existed in the first place.
        // Using regular Destroy may cause weird effects!

        // Add an AntiHasher to prevent this rig's MarrowEntity from being hashed, which would cause strange syncing issues
        rigManager.gameObject.AddComponent<AntiHasher>();

        // Get the controller rig
        var openControllerRig = rigManager.ControllerRig.TryCast<OpenControllerRig>();

        // Set the bone tag to the fusion player tag instead of the default player tag
        var entity = rigManager.physicsRig.marrowEntity;

        entity.Tags.Tags.RemoveAll((Il2CppSystem.Predicate<BoneTagReference>)((tag) => tag.Barcode == MarrowBoneTagReferences.PlayerReference.Barcode));
        
        entity.Tags.Tags.Add(FusionBoneTagReferences.FusionPlayerReference);

        // Clear the controller rig of its observer, bodies, and entity
        var controllerEntity = openControllerRig.GetComponent<MarrowEntity>();
        var controllerBodies = openControllerRig.GetComponentsInChildren<MarrowBody>();
        var controllerTrackers = openControllerRig.GetComponentsInChildren<Tracker>();

        foreach (var body in controllerBodies)
        {
            GameObject.DestroyImmediate(body);
        }

        foreach (var tracker in controllerTrackers)
        {
            GameObject.DestroyImmediate(tracker.gameObject);
        }

        GameObject.DestroyImmediate(controllerEntity);

        // Apply health settings
        var playerHealth = rigManager.health.TryCast<Player_Health>();

        if (playerHealth != null)
        {
            playerHealth.reloadLevelOnDeath = false;
            playerHealth.healthMode = Health.HealthMode.Invincible;

            var newVignetter = GameObject.Instantiate(playerHealth.Vignetter);
            newVignetter.GetComponent<SkinnedMeshRenderer>().enabled = false;
            newVignetter.name = "Vignetter";
            newVignetter.SetActive(false);

            playerHealth.Vignetter = newVignetter;
        }

        // Fix spatial audio
        rigManager.physicsRig.headSfx.mouthSrc.spatialBlend = 1f;

        // Enable body log
        var pullCord = rigManager.physicsRig.GetComponentInChildren<PullCordDevice>(true);
        pullCord._bodyLogEnabled = true;

        // Enable extras
        var health = rigManager.health;
        health._testVisualDamage = true;

        // Remove extra inputs on the controller rig
        openControllerRig.quickmenuEnabled = false;
        openControllerRig._timeInput = false;
        rigManager.remapHeptaRig.doubleJump = false;

        // Remove camera stuff
        var headset = openControllerRig.headset;
        GameObject.DestroyImmediate(headset.GetComponent<AudioListener>());
        GameObject.DestroyImmediate(headset.GetComponent<CameraSettings>());
        GameObject.DestroyImmediate(headset.GetComponent<StreamingController>());
        GameObject.DestroyImmediate(headset.GetComponent<VolumetricRendering>());
        GameObject.DestroyImmediate(headset.GetComponent<UniversalAdditionalCameraData>());
        GameObject.DestroyImmediate(headset.GetComponent<Camera>());

        openControllerRig.cameras = new Il2CppReferenceArray<Camera>(0);
        openControllerRig.onLastCameraUpdate = null;

        headset.tag = "Untagged";

        // Remove unnecessary player art manager
        GameObject.DestroyImmediate(rigManager.GetComponentInChildren<PlayerAvatarArt>(true));

        // Disable ammo trigger
        var ammoReceiver = rigManager.GetComponentInChildren<InventoryAmmoReceiver>();
        ammoReceiver.GetComponent<Collider>().enabled = false;

        // Prevent player rep inventory stuff
        var leftPhysHand = rigManager.physicsRig.leftHand.GetComponent<PhysHand>();
        var rightPhysHand = rigManager.physicsRig.rightHand.GetComponent<PhysHand>();

        leftPhysHand.inventoryPlug.enabled = false;
        leftPhysHand.inventoryPlug.gameObject.SetActive(false);

        rightPhysHand.inventoryPlug.enabled = false;
        rightPhysHand.inventoryPlug.gameObject.SetActive(false);

        // Remove unnecessary controller components
        GameObject.DestroyImmediate(rigManager.ControllerRig.leftController.GetComponent<UIControllerInput>());
        GameObject.DestroyImmediate(rigManager.ControllerRig.rightController.GetComponent<UIControllerInput>());

        Internal_ClearHaptor(rigManager.ControllerRig.leftController.GetComponent<Haptor>());
        Internal_ClearHaptor(rigManager.ControllerRig.rightController.GetComponent<Haptor>());

        // Add impact properties for blunt + stabbing
        PersistentAssetCreator.SetupImpactProperties(rigManager);

        // Spatialize wind audio
        DelayUtilities.InvokeDelayed(() => { Internal_SpatializeWind(rigManager.GetComponentInChildren<WindBuffetSFX>()); }, 5);
    }

    private static void Internal_ClearHaptor(Haptor haptor)
    {
        haptor.hapticsAllowed = false;
        haptor.low_thr_freq = 0f;
        haptor.hap_duration = 0f;
        haptor.hap_frequency = 0f;
        haptor.hap_amplitude = 0f;
        haptor.hap_calc_t = 0f;
        haptor.hap_click_down_t = 0f;
        haptor.hap_click_down_frequency = 0f;
        haptor.hap_click_down_amplitude = 0f;
        haptor.hap_click_up_t = 0f;
        haptor.hap_click_up_frequency = 0f;
        haptor.hap_click_up_amplitude = 0f;
        haptor.hap_tap_duration = 0f;
        haptor.hap_tap_frequency = 0f;
        haptor.hap_tap_amplitude = 0f;
        haptor.hap_knock_duration = 0f;
        haptor.hap_knock_frequency = 0f;
        haptor.hap_knock_amplitude = 0f;
        haptor.hap_hit_mod = 0f;
        haptor.hap_hit_frequency = 0f;
        haptor.sin_gateCount = 0;
        haptor.hap_softSin_length = 0f;
        haptor.hap_softSin_freq = 0f;
        haptor.hap_max_softSin_amp = 0f;
        haptor.hap_min_softSin_amp = 0f;
        haptor.hap_hardSin_length = 0f;
        haptor.hap_hardSin_freq = 0f;
        haptor.hap_max_hardSin_amp = 0f;
        haptor.hap_min_hardSin_amp = 0f;

        haptor.enabled = false;
    }

    private static void Internal_SpatializeWind(WindBuffetSFX sfx)
    {
        if (sfx == null)
        {
            return;
        }

        if (!sfx._buffetSrc)
        {
            return;
        }

        sfx._buffetSrc.spatialBlend = 1f;
    }
}