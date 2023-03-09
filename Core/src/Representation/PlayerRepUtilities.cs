using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SLZ.Rig;

using UnhollowerRuntimeLib;

using UnityEngine;

using LabFusion.Utilities;
using LabFusion.Data;
using LabFusion.Network;
using LabFusion.Syncables;
using LabFusion.Extensions;

using SLZ;
using SLZ.Interaction;
using MelonLoader;
using LabFusion.Grabbables;
using SLZ.Marrow.Utilities;
using SLZ.Marrow.Warehouse;
using SLZ.Bonelab;
using SLZ.Marrow;
using SLZ.UI;
using SLZ.Utilities;
using SLZ.VRMK;
using UnhollowerBaseLib;
using UnityEngine.Events;
using UnityEngine.Rendering;
using SLZ.SFX;
using UnityEngine.Rendering.Universal;
using SLZ.Player;

namespace LabFusion.Representation {
    public static class PlayerRepUtilities {
        public const string PolyBlankBarcode = "c3534c5a-94b2-40a4-912a-24a8506f6c79";

        public static bool IsLocalPlayer(this RigManager rig) {
            if (!RigData.HasPlayer)
                return true;

            return rig == RigData.RigReferences.RigManager;
        }

        public static bool TryGetRigInfo(RigManager rig, out byte smallId, out RigReferenceCollection references) {
            smallId = 0;
            references = null;
            
            if (rig == RigData.RigReferences.RigManager) {
                smallId = PlayerIdManager.LocalSmallId;
                references = RigData.RigReferences;
                return true;
            }
            else if (PlayerRepManager.TryGetPlayerRep(rig, out var rep)) {
                smallId = rep.PlayerId.SmallId;
                references = rep.RigReferences;
                return true;
            }

            return false;
        }

        public static bool TryGetReferences(byte smallId, out RigReferenceCollection references) {
            references = null;

            if (smallId == PlayerIdManager.LocalSmallId) {
                references = RigData.RigReferences;
                return true;
            }
            else if (PlayerRepManager.TryGetPlayerRep(smallId, out var rep)) {
                references = rep.RigReferences;
                return true;
            }

            return false;
        }

        public static bool FindAttachedPlayer(Grip grip, out byte smallId, out RigReferenceCollection references, out bool isAvatarGrip) {
            smallId = 0;
            references = null;
            isAvatarGrip = false;

            if (grip == null)
                return false;

            var rig = grip.GetComponentInParent<RigManager>();
            if (rig != null)
                isAvatarGrip = grip.GetComponentInParent<SLZ.VRMK.Avatar>();

            return TryGetRigInfo(rig, out smallId, out references);
        }

        public static void CreateNewRig(Action<RigManager> onRigCreated) {
            if (MarrowSettings.RuntimeInstance == null)
                return;

            var crate = MarrowSettings.RuntimeInstance.DefaultPlayerRig.Crate;
            if (crate == null)
                return;

            crate.LoadAsset((Action<GameObject>)((go) => Internal_OnLoadPlayer(go, onRigCreated)));
        }

        private static void Internal_OnLoadPlayer(GameObject asset, Action<RigManager> onRigCreated) {
            // Create a temporary parent that is disabled
            GameObject tempParent = new GameObject();
            tempParent.SetActive(false);

            var rigAsset = asset.GetComponentInChildren<RigManager>().gameObject;

            var go = GameObject.Instantiate(rigAsset, tempParent.transform);
            go.name = PlayerRepManager.PlayerRepName;
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

            // Add ammo. If theres no ammo in each category it wont set cartridges properly when grabbing guns
            var ammoInventory = rigManager.AmmoInventory;
            var count = 100000;
            ammoInventory.AddCartridge(ammoInventory.lightAmmoGroup, count);

            var playerHealth = rigManager.health.TryCast<Player_Health>();
            if (playerHealth != null) {
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
            rigManager.bodyVitals.hasBodyLog = true;
            rigManager.bodyVitals.bodyLogFlipped = true;
            rigManager.bodyVitals.bodyLogEnabled = true;

            // Destroy unnecessary data manager
            GameObject.DestroyImmediate(rigManager.uiRig.transform.Find("DATAMANAGER").gameObject);

            // Disable extra rigs
            GameObject.DestroyImmediate(rigManager.GetComponent<LineMesh>());
            GameObject.DestroyImmediate(rigManager.GetComponent<CheatTool>());
            GameObject.DestroyImmediate(rigManager.GetComponent<UtilitySpawnables>());
            GameObject.DestroyImmediate(rigManager.GetComponent<TempTextureRef>());
            GameObject.DestroyImmediate(rigManager.GetComponent<RigVolumeSettings>());
            GameObject.DestroyImmediate(rigManager.GetComponent<ForceLevels>());
            GameObject.DestroyImmediate(rigManager.GetComponent<Volume>());

            var screenOptions = rigManager.GetComponent<RigScreenOptions>();
            GameObject.DestroyImmediate(screenOptions.cam.gameObject);
            GameObject.DestroyImmediate(screenOptions.OverlayCam.gameObject);
            GameObject.DestroyImmediate(screenOptions);

            rigManager.uiRig.gameObject.SetActive(false);
            rigManager.uiRig.Start();
            rigManager.uiRig.popUpMenu.radialPageView.Start();

            try {
                rigManager.uiRig.popUpMenu.Start();
            }
            catch { }

            rigManager.tutorialRig.gameObject.SetActive(false);

            var spawnGunUI = rigManager.GetComponentInChildren<SpawnGunUI>().gameObject;
            spawnGunUI.SetActive(false);

            rigManager.loadAvatarFromSaveData = false;

            // Remove extra inputs on the controller rig
            rigManager.openControllerRig.primaryEnabled = true;
            rigManager.openControllerRig.jumpEnabled = true;
            rigManager.openControllerRig.quickmenuEnabled = false;
            rigManager.openControllerRig.slowMoEnabled = false;
            rigManager.openControllerRig.autoLiftLegs = true;
            rigManager.openControllerRig.doubleJump = false;

            // Remove camera stuff
            GameObject.DestroyImmediate(rigManager.openControllerRig.m_head.GetComponent<AudioListener>());
            GameObject.DestroyImmediate(rigManager.openControllerRig.m_head.GetComponent<DebugDraw>());
            GameObject.DestroyImmediate(rigManager.openControllerRig.m_head.GetComponent<CameraSettings>());
            GameObject.DestroyImmediate(rigManager.openControllerRig.m_head.GetComponent<XRLODBias>());
            GameObject.DestroyImmediate(rigManager.openControllerRig.m_head.GetComponent<VolumetricPlatformSwitch>());
            GameObject.DestroyImmediate(rigManager.openControllerRig.m_head.GetComponent<StreamingController>());
            GameObject.DestroyImmediate(rigManager.openControllerRig.m_head.GetComponent<VolumetricRendering>());
            GameObject.DestroyImmediate(rigManager.openControllerRig.m_head.GetComponent<UniversalAdditionalCameraData>());
            GameObject.DestroyImmediate(rigManager.openControllerRig.m_head.GetComponent<Camera>());

            rigManager.openControllerRig.cameras = new Il2CppReferenceArray<Camera>(0);
            rigManager.openControllerRig.OnLastCameraUpdate = new UnityEvent();

            rigManager.openControllerRig.m_head.tag = "Untagged";

            // Remove unnecessary player art manager
            GameObject.DestroyImmediate(rigManager.GetComponent<PlayerAvatarArt>());

            // Disable ammo trigger
            rigManager.AmmoInventory.ammoReceiver.GetComponent<Collider>().enabled = false;

            // Prevent player rep inventory stuff
            var leftPhysHand = rigManager.physicsRig.leftHand.GetComponent<PhysHand>();
            var rightPhysHand = rigManager.physicsRig.rightHand.GetComponent<PhysHand>();

            leftPhysHand.inventoryPlug.enabled = false;
            leftPhysHand.inventoryPlug.gameObject.SetActive(false);

            rightPhysHand.inventoryPlug.enabled = false;
            rightPhysHand.inventoryPlug.gameObject.SetActive(false);

            // Remove unnecessary controller components
            GameObject.DestroyImmediate(rigManager.openControllerRig.leftController.GetComponent<UIControllerInput>());
            GameObject.DestroyImmediate(rigManager.openControllerRig.rightController.GetComponent<UIControllerInput>());
            
            Internal_ClearHaptor(rigManager.openControllerRig.leftController.GetComponent<Haptor>());
            Internal_ClearHaptor(rigManager.openControllerRig.rightController.GetComponent<Haptor>());

            // Add impact properties for blunt + stabbing
            PersistentAssetCreator.SetupImpactProperties(rigManager);

            // Apply additions
            PlayerAdditionsHelper.OnCreatedRig(rigManager);

            // Add ammo to the other categories
            MelonCoroutines.Start(Internal_DelayAddAmmo(ammoInventory));

            // Spatialize wind audio
            MelonCoroutines.Start(Internal_SpatializeWind(rigManager.physicsRig.m_head.GetComponent<WindBuffetSFX>()));
        }

        private static IEnumerator Internal_DelayAddAmmo(AmmoInventory inventory) {
            for (var i = 0; i < 2; i++)
                yield return null;

            if (!inventory.IsNOC()) {
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

        private static IEnumerator Internal_SpatializeWind(WindBuffetSFX sfx) {
            for (var i = 0; i < 5; i++)
                yield return null;

            if (!sfx.IsNOC() && sfx._buffetSrc)
                sfx._buffetSrc.spatialBlend = 1f;
        }
    }
}
