using UnityEngine;

using LabFusion.Utilities;
using LabFusion.Data;
using LabFusion.Extensions;
using LabFusion.Marrow;

using Il2Cpp;

using Il2CppSLZ.Interaction;
using Il2CppSLZ.Bonelab;
using Il2CppSLZ.Marrow;
using Il2CppSLZ.VRMK;
using Il2CppSLZ.SFX;
using Il2CppSLZ.Player;
using Il2CppSLZ.Marrow.Utilities;
using Il2CppSLZ.Rig;

using Il2CppInterop.Runtime.InteropTypes.Arrays;

using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Il2CppSLZ.Marrow.Interaction;
using LabFusion.Entities;

namespace LabFusion.Representation;

public static class PlayerRepUtilities
{
    // This should never change, incase other mods rely on it.
    public const string PlayerRepName = "[RigManager (FUSION PlayerRep)]";

    public static bool TryGetRigInfo(RigManager rig, out byte smallId, out RigReferenceCollection references)
    {
        smallId = 0;
        references = null;

        if (NetworkPlayerManager.TryGetPlayer(rig, out var player))
        {
            smallId = player.PlayerId.SmallId;
            references = player.RigReferences;
            return true;
        }

        return false;
    }

    public static bool TryGetReferences(byte smallId, out RigReferenceCollection references)
    {
        references = null;

        if (NetworkPlayerManager.TryGetPlayer(smallId, out var player))
        {
            references = player.RigReferences;
            return true;
        }

        return false;
    }

    public static bool FindAttachedPlayer(Grip grip, out byte smallId, out RigReferenceCollection references, out bool isAvatarGrip)
    {
        smallId = 0;
        references = null;
        isAvatarGrip = false;

        if (grip == null)
            return false;

        var rig = grip.GetComponentInParent<RigManager>();
        if (rig != null)
            isAvatarGrip = grip.GetComponentInParent<Il2CppSLZ.VRMK.Avatar>();

        return TryGetRigInfo(rig, out smallId, out references);
    }

    public static void CreateNewRig(Action<RigManager> onRigCreated)
    {
        if (MarrowSettings.RuntimeInstance == null)
            return;

        var crate = MarrowSettings.RuntimeInstance.DefaultPlayerRig.Crate;
        if (crate == null)
            return;

        crate.LoadAsset((Action<GameObject>)((go) => Internal_OnLoadPlayer(go, onRigCreated)));
    }

    private static void Internal_OnLoadPlayer(GameObject asset, Action<RigManager> onRigCreated)
    {
        // Create a temporary parent that is disabled
        GameObject tempParent = new GameObject();
        tempParent.SetActive(false);

        var rigAsset = asset.GetComponentInChildren<RigManager>().gameObject;

        var go = GameObject.Instantiate(rigAsset, tempParent.transform);
        go.name = PlayerRepName;
        go.SetActive(false);

        if (RigData.RigReferences.RigManager)
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

        // Set the bone tag to the fusion player tag instead of the default player tag
        var entity = rigManager.GetComponent<MarrowEntity>();
        entity.Tags.Tags.RemoveAt(0);
        entity.Tags.Tags.Add(FusionBoneTagReferences.FusionPlayerReference);

        // Remove the player rep's observer.
        // This is what triggers chunks and physics culling, but player reps should NOT do this
        var physHead = rigManager.physicsRig.m_head.GetComponent<MarrowBody>();

        // Get the observer tracker out of the array, and preserve the Entity and Being trackers
        var observerTracker = physHead._trackers[2];
        physHead._trackers = new Tracker[]
        {
            physHead._trackers[0],
            physHead._trackers[1],
        };

        // Delete the tracker GameObject
        GameObject.DestroyImmediate(observerTracker.gameObject);

        // Add ammo. If theres no ammo in each category it wont set cartridges properly when grabbing guns
        var ammoInventory = rigManager.GetComponentInChildren<AmmoInventory>();
        var count = 100000;
        ammoInventory.AddCartridge(ammoInventory.lightAmmoGroup, count);

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

        // Enable extras
        var bodyVitals = rigManager.GetComponentInChildren<BodyVitals>();

        bodyVitals.hasBodyLog = true;
        bodyVitals.bodyLogFlipped = true;
        bodyVitals.bodyLogEnabled = true;

        var health = rigManager.health;
        health._testVisualDamage = true;

        // Destroy unnecessary data manager
        var uiRig = rigManager.GetComponentInChildren<UIRig>();

        GameObject.DestroyImmediate(uiRig.transform.Find("DATAMANAGER").gameObject);

        // Disable extra rigs
        GameObject.DestroyImmediate(rigManager.GetComponent<LineMesh>());
        GameObject.DestroyImmediate(rigManager.GetComponent<CheatTool>());
        GameObject.DestroyImmediate(rigManager.GetComponent<UtilitySpawnables>());
        GameObject.DestroyImmediate(rigManager.GetComponent<TempTextureRef>());
        GameObject.DestroyImmediate(rigManager.GetComponent<RigVolumeSettings>());
        GameObject.DestroyImmediate(rigManager.GetComponent<ForceLevels>());
        GameObject.DestroyImmediate(rigManager.GetComponent<Volume>());

        var screenOptions = rigManager.GetComponentInChildren<RigScreenOptions>();
        GameObject.DestroyImmediate(screenOptions.cam.gameObject);
        GameObject.DestroyImmediate(screenOptions.OverlayCam.gameObject);
        GameObject.DestroyImmediate(screenOptions);

        uiRig.gameObject.SetActive(false);

        var avatarManager = rigManager.GetComponentInChildren<PlayerAvatarManager>();
        avatarManager.loadAvatarFromSaveData = false;

        // Remove extra inputs on the controller rig
        var openControllerRig = rigManager.ControllerRig.TryCast<OpenControllerRig>();
        openControllerRig.quickmenuEnabled = false;
        openControllerRig._timeInput = false;
        rigManager.remapHeptaRig.doubleJump = false;

        // Remove camera stuff
        var headset = openControllerRig.headset;
        GameObject.DestroyImmediate(headset.GetComponent<AudioListener>());
        GameObject.DestroyImmediate(headset.GetComponent<DebugDraw>());
        GameObject.DestroyImmediate(headset.GetComponent<CameraSettings>());
        GameObject.DestroyImmediate(headset.GetComponent<XRLODBias>());
        GameObject.DestroyImmediate(headset.GetComponent<VolumetricPlatformSwitch>());
        GameObject.DestroyImmediate(headset.GetComponent<StreamingController>());
        GameObject.DestroyImmediate(headset.GetComponent<VolumetricRendering>());
        GameObject.DestroyImmediate(headset.GetComponent<UniversalAdditionalCameraData>());
        GameObject.DestroyImmediate(headset.GetComponent<Camera>());

        openControllerRig.cameras = new Il2CppReferenceArray<Camera>(0);
        openControllerRig.OnLastCameraUpdate = new UnityEvent();

        headset.tag = "Untagged";

        // Remove unnecessary player art manager
        GameObject.DestroyImmediate(rigManager.GetComponentInChildren<PlayerAvatarArt>(true));

        // Disable ammo trigger
        ammoInventory.ammoReceiver.GetComponent<Collider>().enabled = false;

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

        // Apply additions
        PlayerAdditionsHelper.OnCreatedRig(rigManager);

        // Add ammo to the other categories
        DelayUtilities.Delay(() => { Internal_DelayedAddAmmo(ammoInventory); }, 2);

        // Spatialize wind audio
        DelayUtilities.Delay(() => { Internal_SpatializeWind(rigManager.physicsRig.m_head.GetComponent<WindBuffetSFX>()); }, 5);
    }

    private static void Internal_DelayedAddAmmo(AmmoInventory inventory)
    {
        if (!inventory.IsNOC())
        {
            var count = 100000;
            inventory.AddCartridge(inventory.heavyAmmoGroup, count);
            inventory.AddCartridge(inventory.mediumAmmoGroup, count);
        }
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
        if (!sfx.IsNOC() && sfx._buffetSrc)
            sfx._buffetSrc.spatialBlend = 1f;
    }
}