using LabFusion.Data;
using LabFusion.Extensions;
using LabFusion.Network;
using LabFusion.Utilities;

using SLZ;
using SLZ.Interaction;
using SLZ.Props;
using SLZ.Rig;

using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TMPro;

using UnityEngine;
using UnityEngine.Animations.Rigging;
using MelonLoader;
using UnhollowerRuntimeLib;

namespace LabFusion.Representation
{
    public class PlayerRep : IDisposable {
        public static readonly Dictionary<byte, PlayerRep> Representations = new Dictionary<byte, PlayerRep>();
        public static readonly Dictionary<RigManager, PlayerRep> Managers = new Dictionary<RigManager, PlayerRep>(new UnityComparer());

        public const float PelvisPinMlp = 0.1f;

        public const float NametagHeight = 0.23f;

        public PlayerId PlayerId { get; private set; }
        public string Username { get; private set; } = "Unknown";

        public RigReferenceCollection RigReferences { get; private set; } = new RigReferenceCollection();

        /// <summary>
        /// Returns true if the transforms of the rep have been created yet.
        /// </summary>
        public bool IsCreated => !RigReferences.RigManager.IsNOC();

        public static Transform[] syncedPoints = new Transform[PlayerRepUtilities.TransformSyncCount];
        public static Transform[] gameworldPoints = new Transform[PlayerRepUtilities.GameworldRigTransformCount];
        public static Transform syncedPlayspace;
        public static Transform syncedPelvis;
        public static BaseController syncedLeftController;
        public static BaseController syncedRightController;

        public SerializedLocalTransform[] serializedLocalTransforms = new SerializedLocalTransform[PlayerRepUtilities.TransformSyncCount];
        public SerializedLocalTransform[] serializedGameworldLocalTransforms = new SerializedLocalTransform[PlayerRepUtilities.GameworldRigTransformCount];
        public SerializedTransform serializedPelvis;

        public Vector3 predictVelocity;
        public PDController pdController;
        public float timeSincePelvisSent;

        public Transform[] repTransforms = new Transform[PlayerRepUtilities.TransformSyncCount];
        public Transform[] gameworldRigTransforms = new Transform[PlayerRepUtilities.GameworldRigTransformCount];

        public OpenControllerRig repControllerRig;
        public Transform repPlayspace;
        public Rigidbody repPelvis;
        public BaseController repLeftController;
        public BaseController repRightController;

        public PullCordDevice pullCord;

        public GameObject repCanvas;
        public Canvas repCanvasComponent;
        public Transform repCanvasTransform;
        public TextMeshProUGUI repNameText;

        public SerializedBodyVitals vitals = null;
        public string avatarId = AvatarWarehouseUtilities.INVALID_AVATAR_BARCODE;

        private bool _hasLockedPosition = false;

        public PlayerRep(PlayerId playerId, string barcode)
        {
            PlayerId = playerId;

            Username = playerId.Username;

            Representations.Add(playerId.SmallId, this);
            avatarId = barcode;

            pdController = new PDController();

            CreateRep();
        }

        public void AttachObject(Handedness handedness, Grip grip) {
            var hand = RigReferences.GetHand(handedness);
            if (hand == null)
                return;

            if (grip) {
                grip.OnGrabConfirm(hand, true);
                RigReferences.SetSnatch(handedness, grip);

                if (grip.GetIl2CppType() != Il2CppType.Of<TargetGrip>()) {
                    grip.FreeJoints(hand);

                    RigReferences.RemoveJoint(handedness);
                    RigReferences.SetClientJoint(handedness, hand.gameObject.AddComponent<ConfigurableJoint>());
                }
            }
        }

        public void DetachObject(Handedness handedness) {
            var hand = RigReferences.GetHand(handedness);
            if (hand == null)
                return;

            var grip = RigReferences.GetSnatch(handedness);
            if (grip)
                grip.ForceDetach(hand);
            else
                hand.DetachObject();

            RigReferences.RemoveJoint(handedness);
        }

        public void OnHandFixedUpdate(Hand hand) {
            var clientJoint = RigReferences.GetClientJoint(hand.handedness);

            if (hand.m_CurrentAttachedGO == null || hand.joint == null) {
                RigReferences.SetSerializedAnchor(hand.handedness, null);
            }
            else {
                if (clientJoint != null) {
                    var anchor = RigReferences.GetSerializedAnchor(hand.handedness);

                    if (anchor != null)
                        anchor.CopyTo(hand, Grip.Cache.Get(hand.m_CurrentAttachedGO), clientJoint);
                    else
                        Grip.Cache.Get(hand.m_CurrentAttachedGO).FreeJoints(hand);
                }

                hand.joint.breakForce = float.PositiveInfinity;
                hand.joint.breakTorque = float.PositiveInfinity;
            }
        }

        public void SwapAvatar(string barcode) {
            avatarId = barcode;

            if (RigReferences.RigManager && !string.IsNullOrWhiteSpace(barcode))
                RigReferences.RigManager.SwapAvatarCrate(barcode, false, (Il2CppSystem.Action<bool>)OnSwapAvatar);
        }

        public void OnSwapAvatar(bool success) {
            if (pullCord) {
                pullCord.PlayAvatarParticleEffects();
                pullCord.PlayClip(pullCord.switchAvatar, pullCord.ap3, pullCord.switchVolume, 4f, false);
            }
        }

        public void SetVitals(SerializedBodyVitals vitals) {
            this.vitals = vitals;
            if (RigReferences.RigManager != null && vitals != null) {
                vitals.CopyTo(RigReferences.RigManager.bodyVitals);
                RigReferences.RigManager.bodyVitals.CalibratePlayerBodyScale();
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

            PlayerRepUtilities.CreateNewRig(OnRigCreated);
        }

        private IEnumerator Internal_DelayInitialAvatar() {
            for (var i = 0; i < 40; i++) {
                if (RigReferences.RigManager.IsNOC())
                    yield break;

                yield return null;
            }

            var rig = RigReferences.RigManager;

            if (vitals != null) {
                vitals.CopyTo(rig.bodyVitals);
                rig.bodyVitals.PROPEGATE_SOFT();
                rig.bodyVitals.CalibratePlayerBodyScale();
            }

            if (!string.IsNullOrWhiteSpace(avatarId))
                rig.SwapAvatarCrate(avatarId);
        }

        public void OnRigCreated(RigManager rig) {
            pullCord = rig.GetComponentInChildren<PullCordDevice>(true);

            // Lock many of the bones in place to increase stability
            foreach (var found in rig.GetComponentsInChildren<ConfigurableJoint>(true))
            {
                found.projectionMode = JointProjectionMode.PositionAndRotation;
                found.projectionDistance = 0.001f;
                found.projectionAngle = 40f;
            }

            var leftHaptor = rig.openControllerRig.leftController.haptor;
            rig.openControllerRig.leftController = rig.openControllerRig.leftController.gameObject.AddComponent<Controller>();
            rig.openControllerRig.leftController.manager = rig.openControllerRig;
            leftHaptor.device_Controller = rig.openControllerRig.leftController;
            rig.openControllerRig.leftController.handedness = Handedness.LEFT;

            var rightHaptor = rig.openControllerRig.rightController.haptor;
            rig.openControllerRig.rightController = rig.openControllerRig.rightController.gameObject.AddComponent<Controller>();
            rig.openControllerRig.rightController.manager = rig.openControllerRig;
            rightHaptor.device_Controller = rig.openControllerRig.rightController;
            rig.openControllerRig.rightController.handedness = Handedness.RIGHT;
            Managers.Add(rig, this);

            repPelvis = rig.physicsRig.m_pelvis.GetComponent<Rigidbody>();
            repPelvis.drag = 0f;
            repPelvis.angularDrag = 0f;
            pdController.OnResetDerivatives(repPelvis);

            repControllerRig = rig.openControllerRig;
            repPlayspace = rig.openControllerRig.vrRoot.transform;

            repLeftController = repControllerRig.leftController;
            repRightController = repControllerRig.rightController;

            RigReferences = new RigReferenceCollection(rig);

            PlayerRepUtilities.FillTransformArray(ref repTransforms, rig);
            PlayerRepUtilities.FillGameworldArray(ref gameworldRigTransforms, rig);

            // Delay avatar switching for safety
            MelonCoroutines.Start(Internal_DelayInitialAvatar());
        }

        public static void OnRecreateReps() {
            foreach (var rep in Representations.Values) {
                rep.CreateRep();
            }
        }

        public void OnHeptaBody2Update() {
            try {
                if (gameworldRigTransforms == null)
                    return;

                if (serializedGameworldLocalTransforms == null)
                    return;

                for (var i = 0; i < PlayerRepUtilities.GameworldRigTransformCount; i++)
                {
                    if (gameworldRigTransforms[i].IsNOC())
                        break;

                    if (serializedGameworldLocalTransforms[i] == null)
                        break;

                    var pos = serializedGameworldLocalTransforms[i].position.ToVector3();
                    var rot = serializedGameworldLocalTransforms[i].rotation.Expand();

                    gameworldRigTransforms[i].localPosition = pos;
                    gameworldRigTransforms[i].localRotation = rot;
                }
            }
            catch
            {
            }
        }

        public void OnUpdateNametags() {
            if (!repCanvasTransform.IsNOC()) {
                repCanvasTransform.position = RigReferences.RigManager.physicsRig.m_head.transform.position + Vector3.up * NametagHeight * RigReferences.RigManager.avatar.height;

                if (!RigData.RigReferences.RigManager.IsNOC())
                    repCanvasTransform.rotation = Quaternion.LookRotation(Vector3.Normalize(repCanvasTransform.position - RigData.RigReferences.RigManager.physicsRig.m_head.position), Vector3.up);
            }
        }

        public void OnControllerRigUpdate() {
            try
            {
                if (repTransforms == null)
                    return;

                if (serializedLocalTransforms == null)
                    return;

                for (var i = 0; i < PlayerRepUtilities.TransformSyncCount; i++)
                {
                    if (repTransforms[i].IsNOC())
                        break;

                    repTransforms[i].localPosition = serializedLocalTransforms[i].position.ToVector3();
                    repTransforms[i].localRotation = serializedLocalTransforms[i].rotation.Expand();
                }
            }
            catch {
                // Literally no reason this should happen but it does
                // Doesn't cause anything soooo
            }
        }

        public void OnPelvisPin() {
            try {
                // Stop pelvis
                if (repPelvis.IsNOC())
                    return;

                // Move position with prediction
                if (Time.realtimeSinceStartup - timeSincePelvisSent <= 1.5f) {
                    serializedPelvis.position += predictVelocity * Time.fixedDeltaTime;

                    _hasLockedPosition = false;
                }
                else if (!_hasLockedPosition) {
                    serializedPelvis.position = repPelvis.transform.position;
                    predictVelocity = Vector3.zero;

                    _hasLockedPosition = true;
                }

                // Apply velocity
                var rigManager = RigReferences.RigManager;

                // Seats will cause issues due to jointing
                if (SafetyUtilities.IsValidTime && !rigManager.activeSeat) {
                    var pos = serializedPelvis.position;
                    var rot = serializedPelvis.rotation.Expand();

                    repPelvis.AddForce(pdController.GetForce(repPelvis, pos), ForceMode.Acceleration);

                    // We only want to apply angular force when ragdolled
                    if (rigManager.physicsRig.torso.spineInternalMult <= 0f) {
                        repPelvis.AddTorque(pdController.GetTorque(repPelvis, rot), ForceMode.Acceleration);
                    }
                    else
                        pdController.OnResetRotDerivatives(repPelvis);
                }
                else
                    pdController.OnResetDerivatives(repPelvis);

                // Check for stability teleport
                if (!RigReferences.RigManager.IsNOC()) {
                    float distSqr = (repPelvis.transform.position - serializedPelvis.position).sqrMagnitude;
                    if (distSqr > (2f * (predictVelocity.magnitude + 1f))) {
                        // Get teleport position
                        var pos = serializedPelvis.position;
                        var physRig = RigReferences.RigManager.physicsRig;

                        // Offset
                        pos += physRig.feet.transform.position - physRig.m_pelvis.position;
                        pos += physRig.footballRadius * -physRig.m_pelvis.up;

                        RigReferences.RigManager.Teleport(pos);

                        pdController.OnResetDerivatives(repPelvis);

                        // Reset locosphere and knee pos so the rig doesn't get stuck
                        physRig.knee.transform.position = serializedPelvis.position;
                        physRig.feet.transform.position = serializedPelvis.position;
                    }
                }
            }
            catch {
                // Just ignore these. Don't really matter.
            }
        }

        private static bool TrySendGameworldRep()
        {
            try
            {
                if (gameworldPoints == null || PlayerIdManager.LocalId == null)
                    return false;

                for (var i = 0; i < gameworldPoints.Length; i++)
                {
                    if (gameworldPoints[i].IsNOC())
                        return false;
                }

                using (var writer = FusionWriter.Create())
                {
                    using (var data = PlayerRepGameworldData.Create(PlayerIdManager.LocalSmallId, gameworldPoints))
                    {
                        writer.Write(data);

                        using (var message = FusionMessage.Create(NativeMessageTag.PlayerRepGameworld, writer))
                        {
                            MessageSender.BroadcastMessageExceptSelf(NetworkChannel.Unreliable, message);
                        }
                    }
                }

                return true;
            }
            catch (Exception e)
            {
#if DEBUG
                FusionLogger.Error($"Failed sending gameworld transforms with reason: {e.Message}\nTrace:{e.StackTrace}");
#endif
            }
            return false;
        }

        private static bool TrySendRep() {
            try {
                if (syncedPoints == null || PlayerIdManager.LocalId == null)
                    return false;

                for (var i = 0; i < syncedPoints.Length; i++) {
                    if (syncedPoints[i].IsNOC())
                        return false;
                }

                using (var writer = FusionWriter.Create()) {
                    using (var data = PlayerRepTransformData.Create(PlayerIdManager.LocalSmallId, syncedPoints, syncedPelvis, syncedPlayspace, syncedLeftController, syncedRightController)) {
                        writer.Write(data);

                        using (var message = FusionMessage.Create(NativeMessageTag.PlayerRepTransform, writer)) {
                            MessageSender.BroadcastMessageExceptSelf(NetworkChannel.Unreliable, message);
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
            if (NetworkInfo.HasServer) {
                if (!TrySendRep())
                    OnCachePlayerTransforms();
                else if (!RigData.RigReferences.RigManager.IsNOC() && RigData.RigReferences.RigManager.activeSeat) {
                    TrySendGameworldRep();
                }
            }
        }

        public void OnRepFixedUpdate() {
            OnPelvisPin();

            OnHandFixedUpdate(RigReferences.LeftHand);
            OnHandFixedUpdate(RigReferences.RightHand);

            if (RigReferences.RigManager.IsNOC() || !RigReferences.RigManager.activeSeat) {
                for (var i = 0; i < serializedGameworldLocalTransforms.Length; i++)
                    serializedGameworldLocalTransforms[i] = null;
            }
        }

        public void OnRepLateUpdate() {
            OnUpdateNametags();
        }

        public static void OnFixedUpdate() {
            foreach (var rep in Representations.Values)
                rep.OnRepFixedUpdate();
        }

        public static void OnLateUpdate()
        {
            foreach (var rep in Representations.Values)
                rep.OnRepLateUpdate();
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
            if (!RigReferences.RigManager.IsNOC())
                GameObject.Destroy(RigReferences.RigManager.gameObject);

            if (!repCanvas.IsNOC())
                GameObject.Destroy(repCanvas.gameObject);
        }

        public static void OnCachePlayerTransforms() {
            if (RigData.RigReferences.RigManager.IsNOC())
                return;

            syncedPelvis = RigData.RigReferences.RigManager.physicsRig.m_pelvis;
            syncedPlayspace = RigData.RigReferences.RigManager.openControllerRig.vrRoot.transform;
            syncedLeftController = RigData.RigReferences.RigManager.openControllerRig.leftController;
            syncedRightController = RigData.RigReferences.RigManager.openControllerRig.rightController;

            PlayerRepUtilities.FillTransformArray(ref syncedPoints, RigData.RigReferences.RigManager);
            PlayerRepUtilities.FillGameworldArray(ref gameworldPoints, RigData.RigReferences.RigManager);
        }
    }
}
