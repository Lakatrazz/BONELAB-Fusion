using SLZ.Combat;
using SLZ.Data;
using SLZ.Rig;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TMPro;

using UnityEngine;
using UnityEngine.Audio;

namespace LabFusion.Utilities {
    internal static class PersistentAssetCreator {
        // ALL FONTS AT THE START:
        // - arlon-medium SDF
        // - nasalization-rg SDF
        private const string _targetFont = "arlon-medium";

        internal static SurfaceData BloodSurfaceData { get; private set; }
        internal static TMP_FontAsset Font { get; private set; }

        private static AudioMixerGroup _musicMixer;
        private static AudioMixerGroup _sfxMixer;

        internal static AudioMixerGroup MusicMixer {
            get {
                return _musicMixer;
            }
        }
        internal static AudioMixerGroup SFXMixer {
            get {
                return _sfxMixer;
            }
        }

        private static Action<AudioMixerGroup> _onMusicMixerLoaded = null;
        private static Action<AudioMixerGroup> _onSFXMixerLoaded = null;

        internal static void OnLateInitializeMelon() {
            CreateSurfaceData();
            CreateTextFont();
        }

        internal static void OnMainSceneInitialized() {
            GetAllMixers();
        }

        // Thanks to https://bonelab.thunderstore.io/package/Maranara/Mixer_Fixer/
        private static void GetAllMixers() {
            if (_sfxMixer != null && _musicMixer != null)
                return;

            AudioMixerGroup[] groups = Resources.FindObjectsOfTypeAll<AudioMixerGroup>();

            foreach (var group in groups) {
                switch (group.name) {
                    case "Music":
                        _musicMixer = group;
                        break;
                    case "SFX":
                        _sfxMixer = group;
                        break;
                }
            }

            if (_musicMixer != null)
                _onMusicMixerLoaded?.Invoke(_musicMixer);

            if (_sfxMixer != null)
                _onSFXMixerLoaded?.Invoke(_sfxMixer);

            _onMusicMixerLoaded = null;
            _onSFXMixerLoaded = null;
        }

        public static void HookOnMusicMixerLoaded(Action<AudioMixerGroup> action) {
            if (_musicMixer != null) {
                action?.Invoke(_musicMixer);
            }
            else {
                _onMusicMixerLoaded += action;
            }
        }

        public static void HookOnSFXMixerLoaded(Action<AudioMixerGroup> action)
        {
            if (_sfxMixer != null) {
                action?.Invoke(_sfxMixer);
            }
            else {
                _onSFXMixerLoaded += action;
            }
        }

        private static void CreateTextFont() {
            // I don't want to use asset bundles in this mod.
            // Is this a bad method? Sure, but it only runs once.
            // So WHO CARES!
            var fonts = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
            foreach (var font in fonts) {
                if (font.name.ToLower().Contains(_targetFont)) {
                    Font = font;
                    break;
                }
            }

            // Make sure we at least have a font
            if (Font == null) {
#if DEBUG
                FusionLogger.Error($"Failed finding the {_targetFont} font! Defaulting to the first font in the game!");
#endif

                Font = fonts[0];
            }
        }

        private static void CreateSurfaceData() {
            BloodSurfaceData = ScriptableObject.CreateInstance<SurfaceData>();
            BloodSurfaceData.name = "Fusion Blood Surface";
            BloodSurfaceData.PenetrationResistance = 0.59f;
            BloodSurfaceData.megaPascal = 4f;
            BloodSurfaceData.isFlammable = false;
            BloodSurfaceData.fireResistance = 0f;
            BloodSurfaceData.ParicleColorTint = new Color(0.1686275f, 0.003372546f, 0.003372546f, 1f);

            var physMaterial = new PhysicMaterial("Fusion Physic Material")
            {
                dynamicFriction = 0.45f,
                staticFriction = 0.63f,
                bounciness = 0.35f,
                frictionCombine = PhysicMaterialCombine.Multiply,
                bounceCombine = PhysicMaterialCombine.Multiply
            };
            BloodSurfaceData.physicMaterial = physMaterial;
        }

        internal static void SetupImpactProperties(RigManager rig) {
            var physRig = rig.physicsRig;
            var rigidbodies = physRig.GetComponentsInChildren<Rigidbody>(true);

            for (var i = 0; i < rigidbodies.Length; i++) {
                var rb = rigidbodies[i];

                // Ignore specific rigidbodies
                var go = rb.gameObject;
                if (go == physRig.knee || go == physRig.feet) {
                    continue;
                }

                if (i == 0) {
                    var impactManager = go.AddComponent<ImpactPropertiesManager>();
                    impactManager.surfaceData = BloodSurfaceData;
                    impactManager.DecalMeshObj = null;
                    impactManager.decalType = ImpactPropertiesVariables.DecalType.none;
                }

                var properties = go.AddComponent<ImpactProperties>();
                properties.surfaceData = BloodSurfaceData;
                properties.DecalMeshObj = null;
                properties.decalType = ImpactPropertiesVariables.DecalType.none;
            }
        }
    }
}
