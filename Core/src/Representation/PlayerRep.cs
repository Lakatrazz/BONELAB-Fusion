using LabFusion.Data;
using LabFusion.Extensions;
using LabFusion.Network;
using LabFusion.Utilities;
using SLZ;
using SLZ.Marrow.Warehouse;
using SLZ.Rig;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace LabFusion.Representation
{
    public class PlayerRep : IDisposable {
        public static readonly Dictionary<byte, PlayerRep> Representations = new Dictionary<byte, PlayerRep>();
        public static readonly Dictionary<RigManager, PlayerRep> Managers = new Dictionary<RigManager, PlayerRep>(new UnityComparer());

        public PlayerId PlayerId { get; private set; }
        public string Username { get; private set; } = "Unknown";

        public static Transform[] syncedPoints = new Transform[PlayerRepUtilities.TransformSyncCount];
        public static Transform syncedPlayspace;
        public static Transform syncedPelvis;
        public static BaseController syncedLeftController;
        public static BaseController syncedRightController;

        public SerializedTransform[] serializedTransforms = new SerializedTransform[PlayerRepUtilities.TransformSyncCount];
        public SerializedTransform serializedPelvis;

        public Vector3 predictVelocity;
        public float timeSincePelvisSent;

        public Transform[] repTransforms = new Transform[PlayerRepUtilities.TransformSyncCount];
        public OpenControllerRig repControllerRig;
        public Transform repPlayspace;
        public Rigidbody repPelvis;
        public BaseController repLeftController;
        public BaseController repRightController;

        public RigManager rigManager;

        public GameObject repCanvas;
        public Canvas repCanvasComponent;
        public Transform repCanvasTransform;
        public TextMeshProUGUI repNameText;

        public SerializedBodyVitals vitals = null;
        public string avatarId = NetworkUtilities.InvalidAvatarId;

        public PlayerRep(PlayerId playerId, string barcode)
        {
            PlayerId = playerId;

            if (FusionMod.CurrentNetworkLayer != null)
                Username = FusionMod.CurrentNetworkLayer.GetUsername(playerId.LongId);

            Representations.Add(playerId.SmallId, this);
            avatarId = barcode;

            CreateRep();
        }

        public void SwapAvatar(string barcode) {
            avatarId = barcode;

            if (rigManager && !string.IsNullOrWhiteSpace(barcode))
                rigManager.SwapAvatarCrate(barcode, false);
        }

        public void SetVitals(SerializedBodyVitals vitals) {
            this.vitals = vitals;
            if (rigManager != null && vitals != null) {
                vitals.CopyTo(rigManager.bodyVitals);
                rigManager.bodyVitals.CalibratePlayerBodyScale();
            }
        }

        public void CreateRep() {
            // Make sure we don't have any extra objects
            DestroyRep();

            repCanvas = new GameObject("RepCanvas");
            repCanvasComponent = repCanvas.AddComponent<Canvas>();

            repCanvasComponent.renderMode = RenderMode.WorldSpace;
            repCanvasTransform = repCanvas.transform;
            repCanvasTransform.localScale = Vector3.one / 200.0f;

            repNameText = repCanvas.AddComponent<TextMeshProUGUI>();

            repNameText.alignment = TextAlignmentOptions.Midline;
            repNameText.enableAutoSizing = true;

            repNameText.text = Username;

            rigManager = PlayerRepUtilities.CreateNewRig();

            if (vitals != null) {
                vitals.CopyTo(rigManager.bodyVitals);
                rigManager.bodyVitals.CalibratePlayerBodyScale();
            }

            // Lock many of the bones in place to increase stability
            foreach (var found in rigManager.GetComponentsInChildren<ConfigurableJoint>(true)) {
                found.projectionMode = JointProjectionMode.PositionAndRotation;
                found.projectionDistance = 0.001f;
                found.projectionAngle = 40f;
            }

            if (!string.IsNullOrWhiteSpace(avatarId))
                rigManager.SwapAvatarCrate(avatarId);

            var leftHaptor = rigManager.openControllerRig.leftController.haptor;
            rigManager.openControllerRig.leftController = rigManager.openControllerRig.leftController.gameObject.AddComponent<Controller>();
            leftHaptor.device_Controller = rigManager.openControllerRig.leftController;
            rigManager.openControllerRig.leftController.handedness = Handedness.LEFT;

            var rightHaptor = rigManager.openControllerRig.rightController.haptor;
            rigManager.openControllerRig.rightController = rigManager.openControllerRig.rightController.gameObject.AddComponent<Controller>();
            rightHaptor.device_Controller = rigManager.openControllerRig.rightController;
            rigManager.openControllerRig.rightController.handedness = Handedness.RIGHT;

            Managers.Add(rigManager, this);

            repPelvis = rigManager.physicsRig.m_pelvis.GetComponent<Rigidbody>();
            repControllerRig = rigManager.openControllerRig;
            repPlayspace = rigManager.openControllerRig.vrRoot.transform;

            repLeftController = repControllerRig.leftController;
            repRightController = repControllerRig.rightController;

            PlayerRepUtilities.FillTransformArray(ref repTransforms, rigManager);
        }

        public static void OnRecreateReps(bool isSceneLoad = false) {
            foreach (var rep in Representations.Values) {
                rep.CreateRep();
            }
        }

        public void OnUpdateTransforms() {
            try
            {
                if (repTransforms == null)
                    return;

                if (serializedTransforms == null)
                    return;

                for (var i = 0; i < PlayerRepUtilities.TransformSyncCount; i++)
                {
                    if (repTransforms[i] == null)
                        break;

                    repTransforms[i].localPosition = serializedTransforms[i].position;
                    repTransforms[i].localRotation = serializedTransforms[i].rotation.Expand();
                }

                if (repCanvasTransform) {
                    repCanvasTransform.position = repTransforms[0].position + Vector3.up * 0.4f;

                    if (RigData.RigManager)
                        repCanvasTransform.rotation = Quaternion.LookRotation(Vector3.Normalize(repCanvasTransform.position - RigData.RigManager.physicsRig.m_head.position), Vector3.up);
                }
            }
            catch {
                // Literally no reason this should happen but it does
                // Doesn't cause anything soooo
            }
        }

        public void OnUpdateVelocity() {
            try {
                // Stop pelvis
                if (repPelvis == null)
                    return;

                // Move position with prediction
                if (Time.realtimeSinceStartup - timeSincePelvisSent <= 2.5f)
                    serializedPelvis.position += predictVelocity;

                // Apply velocity
                if (Time.timeScale > 0f && Time.deltaTime > 0f && Time.fixedDeltaTime > 0f)
                    repPelvis.velocity = PhysXUtils.GetLinearVelocity(repPelvis.transform.position, serializedPelvis.position);

                // Check for stability teleport
                if (RigData.RigManager && RigData.RigManager.avatar)
                {
                    float distSqr = (repPelvis.transform.position - serializedPelvis.position).sqrMagnitude;
                    if (distSqr > (1.2f * RigData.RigManager.avatar.height))
                    {
                        rigManager.Teleport(serializedPelvis.position);
                    }
                }
            }
            catch {
                // I give up idk there was so many null checks and yet still an error when missed
                // Il2?
            }
        }

        private static bool TrySendRep() {
            try {
                if (syncedPoints == null)
                    return false;

                for (var i = 0; i < syncedPoints.Length; i++) {
                    if (syncedPoints[i] == null)
                        return false;
                }

                using (var writer = FusionWriter.Create()) {
                    using (var data = PlayerRepTransformData.Create(PlayerId.SelfId.SmallId, syncedPoints, syncedPelvis, syncedPlayspace, syncedLeftController, syncedRightController)) {
                        writer.Write(data);

                        using (var message = FusionMessage.Create(NativeMessageTag.PlayerRepTransform, writer)) {
                            FusionMod.CurrentNetworkLayer.BroadcastMessage(NetworkChannel.Unreliable, message);
                        }
                    }
                }

                return true;
            } 
            catch (Exception e) {
#if DEBUG
                FusionLogger.Error($"Failed sending player transforms with reason: {e.Message}\nTrace:{e.StackTrace}");
#endif
            }
            return false;
        }

        public static void OnSyncRep() {
            if (NetworkUtilities.HasServer) {
                if (!TrySendRep())
                    OnCachePlayerTransforms();
            }
        }

        public static void OnUpdateTrackers() {

        }

        /// <summary>
        /// Destroys anything about the PlayerRep and frees it from memory.
        /// </summary>
        public void Dispose() {
            Representations.Remove(PlayerId.SmallId);

            DestroyRep();

            GC.SuppressFinalize(this);

#if DEBUG
            FusionLogger.Log($"Disposed PlayerRep with small id {PlayerId.SmallId}");
#endif
        }

        /// <summary>
        /// Destroys the GameObjects of the PlayerRep. Does not free it from memory or remove it from its slots. Use Dispose for that.
        /// </summary>
        public void DestroyRep() {
            if (rigManager != null)
                GameObject.Destroy(rigManager.gameObject);

            if (repCanvas != null)
                GameObject.Destroy(repCanvas.gameObject);
        }

        public static void OnCachePlayerTransforms() {
            if (RigData.RigManager == null)
                return;

            syncedPelvis = RigData.RigManager.physicsRig.m_pelvis;
            syncedPlayspace = RigData.RigManager.openControllerRig.vrRoot.transform;
            syncedLeftController = RigData.RigManager.openControllerRig.leftController;
            syncedRightController = RigData.RigManager.openControllerRig.rightController;

            PlayerRepUtilities.FillTransformArray(ref syncedPoints, RigData.RigManager);
        }
    }
}
