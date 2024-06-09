using LabFusion.Utilities;

using Il2CppSLZ.Interaction;

using UnityEngine;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppSLZ.Marrow.Audio;

namespace LabFusion.UI
{
    public class FusionUIMachine : MonoBehaviour
    {
        public FusionUIMachine(IntPtr intPtr) : base(intPtr) { }

        private void Awake()
        {
            // Create grips
            PersistentAssetCreator.HookOnSoftGrabLoaded((p) =>
            {
                var root = GetGripRoot();

                foreach (var collider in root.GetComponentsInChildren<Collider>())
                {
                    if (!Grip.Cache.Get(collider.gameObject))
                    {
                        var genericGrip = collider.gameObject.AddComponent<GenericGrip>();
                        genericGrip.isThrowable = true;
                        genericGrip.ignoreGripTargetOnAttach = false;
                        genericGrip.additionalGripColliders = new Il2CppReferenceArray<Collider>(0);
                        genericGrip.handleAmplifyCurve = AnimationCurve.Linear(0f, 1f, 0f, 1f);
                        genericGrip.gripOptions = InteractionOptions.MultipleHands;
                        genericGrip.priority = 1f;
                        genericGrip.handPose = p;
                        genericGrip.minBreakForce = float.PositiveInfinity;
                        genericGrip.maxBreakForce = float.PositiveInfinity;
                        genericGrip.defaultGripDistance = float.PositiveInfinity;
                        genericGrip.radius = 0.24f;
                    }
                }

                foreach (var rb in transform.GetComponentsInChildren<Rigidbody>())
                {
                    rb.gameObject.AddComponent<InteractableHost>();
                }
            });

            // Add the panel view
            Transform panel = transform.Find("PANELVIEW");
            AddPanelView(panel.gameObject);

            // Setup the UI trigger
            UIMachineUtilities.CreateUITrigger(panel.Find("CANVAS").gameObject, transform.Find("uiTrigger").gameObject);

            // Setup audio
            AudioSource[] sources = gameObject.GetComponentsInChildren<AudioSource>(true);

            foreach (var source in sources)
            {
                source.outputAudioMixerGroup = Audio3dManager.diegeticMusic;
            }

            OnAwake();
        }

        protected virtual void OnAwake() { }

        protected virtual void AddPanelView(GameObject panel) { }

        protected virtual Transform GetGripRoot() { return null; }
    }
}
