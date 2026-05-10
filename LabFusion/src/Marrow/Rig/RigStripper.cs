using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Interaction;
using Il2CppSLZ.Marrow.Warehouse;

using LabFusion.Utilities;

using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace LabFusion.Marrow.Rig;

/// <summary>
/// Provides functionality to strip unnecessary components from a spawned RigManager for use in multiplayer.
/// </summary>
public static class RigStripper
{
    // Notice when stripping components from the RigManager:
    // Using regular "Destroy" will result in issues, as it destroys the component in the next frame
    // This means that the Rig being enabled would cause the components to still run their awake calls
    // Instead, "DestroyImmediate" should be used for all components that need to be stripped

    /// <summary>
    /// Invoked when all the main removable components of a rig have been stripped.
    /// Subscribe if additional components need to be removed.
    /// When stripping components, use DestroyImmediate instead of Destroy.
    /// </summary>
    public static event Action<RigManager> OnStripRigManager;

    /// <summary>
    /// Strips a RigManager of all unnecessary components that would interfere with the local player.
    /// </summary>
    /// <param name="rigManager"></param>
    public static void StripRigManager(RigManager rigManager)
    {
        StripPhysicsRig(rigManager.physicsRig);
        StripOpenControllerRig(rigManager.ControllerRig.TryCast<OpenControllerRig>());
        StripHealth(rigManager.health);

        StripAdditionalComponents(rigManager);

        SpatializeRig(rigManager);

        OnStripRigManager?.Invoke(rigManager);
    }

    private static void SpatializeRig(RigManager rigManager)
    {
        // Spatialize the voice sfx
        rigManager.physicsRig.headSfx.mouthSrc.spatialBlend = 1f;

        // Spatialize the wind sounds
        // Wait a few frames as the audio source does not exist yet
        var windBuffetSfx = rigManager.GetComponentInChildren<WindBuffetSFX>(true);

        DelayUtilities.InvokeDelayed(() => { SpatializeWindBuffetSFX(windBuffetSfx); }, 5);
    }

    private static void SpatializeWindBuffetSFX(WindBuffetSFX windBuffetSfx)
    {
        if (windBuffetSfx == null)
        {
            return;
        }

        var source = windBuffetSfx._buffetSrc;

        if (!source)
        {
            return;
        }

        source.spatialBlend = 1f;
    } 

    private static void StripAdditionalComponents(RigManager rigManager)
    {
        // Remove the avatar art manager, as it applies a head offset only meant for first person
        GameObject.DestroyImmediate(rigManager.GetComponentInChildren<PlayerAvatarArt>(true));

        // Prevent interaction with the ammo trigger
        var ammoReceiver = rigManager.GetComponentInChildren<InventoryAmmoReceiver>();
        ammoReceiver.GetComponent<Collider>().enabled = false;
    }

    private static void StripHealth(Health health)
    {
        var playerHealth = health.TryCast<Player_Health>();

        if (playerHealth == null)
        {
            return;
        }

        playerHealth.reloadLevelOnDeath = false;
        playerHealth.healthMode = Health.HealthMode.Invincible;

        var newVignetter = GameObject.Instantiate(playerHealth.Vignetter);
        newVignetter.GetComponent<SkinnedMeshRenderer>().enabled = false;
        newVignetter.name = "Vignetter";
        newVignetter.SetActive(false);

        playerHealth.Vignetter = newVignetter;
    }

    private static void StripPhysicsRig(PhysicsRig physicsRig)
    {
        // Strip the regular player tag but keep the being tag
        // Spawned rigs should not trigger regular zone events or culling functionality
        var entity = physicsRig.marrowEntity;

        entity.Tags.Tags.RemoveAll((Il2CppSystem.Predicate<BoneTagReference>)((tag) => tag.Barcode == MarrowBoneTagReferences.PlayerReference.Barcode));

        // Remove unnecessary components from the hands
        StripHand(physicsRig.leftHand);
        StripHand(physicsRig.rightHand);
    }

    private static void StripHand(Hand hand)
    {
        var physHand = hand.GetComponent<PhysHand>();

        physHand.inventoryPlug.enabled = false;
        physHand.inventoryPlug.gameObject.SetActive(false);
    }

    private static void StripOpenControllerRig(OpenControllerRig openControllerRig)
    {
        // Remove all MarrowEntity components from the controller rig
        // These are used for tracking chunks, which stripped rigs should not do
        var entity = openControllerRig.GetComponent<MarrowEntity>();
        var bodies = openControllerRig.GetComponentsInChildren<MarrowBody>();
        var trackers = openControllerRig.GetComponentsInChildren<Tracker>();

        foreach (var body in bodies)
        {
            GameObject.DestroyImmediate(body);
        }

        foreach (var tracker in trackers)
        {
            GameObject.DestroyImmediate(tracker.gameObject);
        }

        GameObject.DestroyImmediate(entity);

        // Remove additional inputs that should not be triggered from the controller rig
        openControllerRig.quickmenuEnabled = false;
        openControllerRig._timeInput = false;

        // Remove camera components so that it doesn't waste performance and override the normal camera
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

        // Remove unnecessary components from the controllers
        StripController(openControllerRig.leftController);
        StripController(openControllerRig.rightController);
    }

    private static void StripController(BaseController controller)
    {
        StripHaptor(controller.GetComponent<Haptor>());
    }

    private static void StripHaptor(Haptor haptor)
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
}
