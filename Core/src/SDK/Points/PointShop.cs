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
using static SLZ.Interaction.LadderInfo;

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
            PersistentAssetCreator.HookOnSFXMixerLoaded((m) => {
                if (gameObject != null) {
                    AudioSource[] sources = gameObject.GetComponentsInChildren<AudioSource>(true);

                    foreach (var source in sources) {
                        source.outputAudioMixerGroup = m;
                    }
                }
            });
        }
    }
}