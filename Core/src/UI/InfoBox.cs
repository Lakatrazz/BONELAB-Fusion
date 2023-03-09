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

namespace LabFusion.UI
{
    [RegisterTypeInIl2Cpp]
    public sealed class InfoBox : MonoBehaviour {
        private static HandPose _gripPose;

        public InfoBox(IntPtr intPtr) : base(intPtr) { }

        private InfoBoxPanelView _panelView;

        public InfoBoxPanelView PanelView => _panelView;

        public void Awake() {
            // Setup grips
            Transform colliders = transform.Find("Colliders");

            // Get grip pose
            if (_gripPose == null) {
                var poses = Resources.FindObjectsOfTypeAll<HandPose>();

                if (poses.Length > 0) {
                    _gripPose = poses[0];
                    _gripPose.hideFlags = HideFlags.DontUnloadUnusedAsset;
                }
            }

            if (_gripPose != null) {
                foreach (var collider in colliders.GetComponentsInChildren<Collider>()) {
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
            _panelView = panel.gameObject.AddComponent<InfoBoxPanelView>();
        }
    }
}