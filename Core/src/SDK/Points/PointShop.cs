using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

using MelonLoader;

using UnityEngine.UI;

using SLZ.Interaction;

using LabFusion.Data;
using LabFusion.Utilities;

using SLZ.Marrow.Data;

using UnhollowerBaseLib;

namespace LabFusion.SDK.Points
{
    [RegisterTypeInIl2Cpp]
    public sealed class PointShop : MonoBehaviour {
        private static HandPose _gripPose;

        public PointShop(IntPtr intPtr) : base(intPtr) { }

        private PointShopPanelView _panelView;

        public PointShopPanelView PanelView => _panelView;

        public void Awake() {
            // Setup grips
            Transform art = transform.Find("Art");

            // Get grip pose
            if (_gripPose == null) {
                var poses = Resources.FindObjectsOfTypeAll<HandPose>();

                if (poses.Length > 0) {
                    _gripPose = poses[0];
                    _gripPose.hideFlags = HideFlags.DontUnloadUnusedAsset;
                }
            }

            if (_gripPose != null) {
                foreach (var collider in art.GetComponentsInChildren<Collider>()) {
                    if (!Grip.Cache.Get(collider.gameObject)) {
                        var genericGrip = collider.gameObject.AddComponent<GenericGrip>();
                        genericGrip.isThrowable = true;
                        genericGrip.ignoreGripTargetOnAttach = false;
                        genericGrip.additionalGripColliders = new Il2CppReferenceArray<Collider>(0);
                        genericGrip.handleAmplifyCurve = AnimationCurve.Linear(0f, 1f, 0f, 1f);
                        genericGrip.gripOptions = InteractionOptions.MultipleHands;
                        genericGrip.priority = 1f;
                        genericGrip.handPose = _gripPose;
                        genericGrip.minBreakForce = float.PositiveInfinity;
                        genericGrip.maxBreakForce = float.PositiveInfinity;
                        genericGrip.defaultGripDistance = float.PositiveInfinity;
                        genericGrip.radius = 0.24f;
                    }
                }

                foreach (var rb in transform.GetComponentsInChildren<Rigidbody>()) {
                    rb.gameObject.AddComponent<InteractableHost>();
                }
            }

            // Add the panel view
            Transform panel = transform.Find("PANELVIEW");
            _panelView = panel.gameObject.AddComponent<PointShopPanelView>();

            // Setup audio
            AudioSource[] sources = gameObject.GetComponentsInChildren<AudioSource>(true);

            foreach (var source in sources) {
                source.outputAudioMixerGroup = PersistentAssetCreator.SFXMixer;
                source.volume *= 0.8f;
            }
        }
    }

    [RegisterTypeInIl2Cpp]
    public sealed class PointShopUITrigger : MonoBehaviour {
        public static PointShopUITrigger Selected { get; private set; }

        public PointShopUITrigger(IntPtr intPtr) : base(intPtr) { }

        public Button button;

        private Hand _selectedHand = null;
        private bool _isActive = false;

        private void Awake() {
            var boxCollider = GetComponent<BoxCollider>();
            if (boxCollider != null) {
                var size = boxCollider.size;
                size.z *= 4.3f; // Multiplied in code incase this gets replaced with SLZ's UI
                boxCollider.size = size;
            }
        }

        private void OnTriggerEnter(Collider other) {
            if (_isActive)
                return;

            var hand = Hand.Cache.Get(other.gameObject);

            if (hand != null && hand.manager == RigData.RigReferences.RigManager) {
                // Deselect active
                if (Selected != null && Selected != this)
                    Selected.Deselect();

                Select(hand);
            }
        }
        
        private void OnTriggerExit(Collider other) {
            if (!_isActive)
                return;

            if (other.gameObject == _selectedHand.gameObject) {
                Deselect();
            }
        }

        private void OnDisable() {
            if (_isActive)
                Deselect();
        }

        private void Update() {
            if (_isActive) {
                var controller = _selectedHand.Controller;

                if (controller.GetPrimaryInteractionButtonDown()) {
                    Click();
                }
            }
        }

        private void Click() {
            button.OnSubmit(null);

            FusionAudio.Play3D(transform.position, FusionContentLoader.UIConfirm, 0.7f);
        }

        private void Select(Hand hand) {
            _selectedHand = hand;
            _isActive = true;
            Selected = this;
            button.OnSelect(null);
            button.StartColorTween(button.colors.selectedColor, false);

            FusionAudio.Play3D(transform.position, FusionContentLoader.UISelect, 0.7f);
        }

        private void Deselect() {
            _selectedHand = null;
            _isActive = false;
            Selected = null;

            button.OnDeselect(null);
            button.StartColorTween(button.colors.normalColor, false);
        }
    }
}