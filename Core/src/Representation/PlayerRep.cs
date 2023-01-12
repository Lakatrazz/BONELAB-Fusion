using LabFusion.Data;
using LabFusion.Extensions;
using LabFusion.Network;
using LabFusion.Utilities;

using SLZ;
using SLZ.Interaction;
using SLZ.Props;
using SLZ.Rig;
using SLZ.Marrow.Utilities;

using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TMPro;

using UnityEngine;

using MelonLoader;

namespace LabFusion.Representation
{
    public class PlayerRep : IDisposable {
        public static readonly Dictionary<byte, PlayerRep> Representations = new Dictionary<byte, PlayerRep>();
        public static readonly Dictionary<RigManager, PlayerRep> Managers = new Dictionary<RigManager, PlayerRep>(new UnityComparer());

        public const float NametagHeight = 0.23f;
        public const float NameTagDivider = 250f;

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
        public static Transform syncedFootball;
        public static BaseController syncedLeftController;
        public static BaseController syncedRightController;

        public SerializedLocalTransform[] serializedLocalTransforms = new SerializedLocalTransform[PlayerRepUtilities.TransformSyncCount];
        public SerializedLocalTransform[] serializedGameworldLocalTransforms = new SerializedLocalTransform[PlayerRepUtilities.GameworldRigTransformCount];
        public SerializedTransform serializedPelvis;
        public SerializedTransform serializedFootball;

        public float serializedFeetOffset;
        public float serializedCrouchTarget;
        public float serializedSpineCrouchOff;

        public float serializedVrTwist;

        public ControllerRig.TraversalState serializedTravState;
        public ControllerRig.VertState serializedVertState;
        public ControllerRig.VrVertState serializedVrVertState;

        public Vector3 predictVelocity;
        public PDController pelvisPDController;
        public PDController footPDController;
        public float timeSincePelvisSent;

        public Transform[] repTransforms = new Transform[PlayerRepUtilities.TransformSyncCount];
        public Transform[] gameworldRigTransforms = new Transform[PlayerRepUtilities.GameworldRigTransformCount];

        public OpenControllerRig repControllerRig;
        public Transform repPlayspace;
        public Rigidbody repPelvis;
        public Rigidbody repFootBall;
        public BaseController repLeftController;
        public BaseController repRightController;

        public PullCordDevice pullCord;

        public GameObject repCanvas;
        public Canvas repCanvasComponent;
        public Transform repCanvasTransform;
        public TextMeshProUGUI repNameText;

        public SerializedBodyVitals vitals = null;
        public SerializedAvatarStats avatarStats = null;
        public SerializedPlayerSettings playerSettings = null;
        public string avatarId = AvatarWarehouseUtilities.INVALID_AVATAR_BARCODE;

        public SerializedHand serializedLeftHand = null;
        public SerializedHand serializedRightHand = null;

        private bool _hasLockedPosition = false;

        private bool _isAvatarDirty = false;
        private bool _isVitalsDirty = false;

        private bool _isRagdollDirty = false;
        private bool _ragdollState = false;

        private bool _isSettingsDirty = false;
        private bool _isServerDirty = false;

        public PlayerRep(PlayerId playerId)
        {
            PlayerId = playerId;

            Username = playerId.Username;

            Representations.Add(playerId.SmallId, this);

            pelvisPDController = new PDController();
            footPDController = new PDController();

            FusionPreferences.OnServerSettingsChange += OnServerSettingsChange;

            ResetSerializedTransforms();

            CreateRep();
        }

        public void ResetSerializedTransforms() {
            for (var i = 0; i < PlayerRepUtilities.TransformSyncCount; i++) {
                serializedLocalTransforms[i] = new SerializedLocalTransform(Vector3.zero, Quaternion.identity);
            }
        }

        private void OnServerSettingsChange() {
            _isServerDirty = true;
        }

        public void AttachObject(Handedness handedness, Grip grip, bool useCustomJoint = true) {
            var hand = RigReferences.GetHand(handedness);
            if (hand == null)
                return;

            if (grip) {
                // Update snatch grip
                RigReferences.SetSnatch(handedness, grip);

                // Create lock joint
                var joint = hand.gameObject.AddComponent<ConfigurableJoint>();
                joint.xMotion = joint.yMotion = joint.zMotion = joint.angularXMotion = joint.angularYMotion = joint.angularZMotion = ConfigurableJointMotion.Locked;
                joint.projectionMode = JointProjectionMode.PositionAndRotation;
                joint.projectionAngle = 0f;
                joint.projectionDistance = 0f;

                if (grip.HasRigidbody)
                    joint.connectedBody = grip.Host.Rb;

                // Delay grabbing the object and destroying the joint
                MelonCoroutines.Start(Internal_DelayedGrab(joint, hand, handedness, grip, useCustomJoint));
            }
        }

        private IEnumerator Internal_DelayedGrab(Joint joint, Hand hand, Handedness handedness, Grip grip, bool useCustomJoint = true) {
            // Wait a few frames
            for (var i = 0; i < 5; i++) {
                if (RigReferences.GetSnatch(handedness) != null) {
                    // Update last time grabbed, so that the hovering actually updates properly
                    hand.Controller._lastTimeGrabbed = Time.realtimeSinceStartup;

                    yield return null;
                }
                else
                {
                    // Destroy the joint if we are cancelling the grab
                    if (!joint.IsNOC())
                        GameObject.Destroy(joint);

                    yield break;
                }
            }

            // Actually attach the joints
            grip.OnGrabConfirm(hand, true);

            if (useCustomJoint) {
                grip.FreeJoints(hand);

                RigReferences.RemoveJoint(handedness);
                RigReferences.SetClientJoint(handedness, hand.gameObject.AddComponent<ConfigurableJoint>());
            }

            // Destroy the temp joint
            if (!joint.IsNOC())
                GameObject.Destroy(joint);
        }

        public void DetachObject(Handedness handedness) {
            var hand = RigReferences.GetHand(handedness);
            if (hand == null)
                return;

            var grip = RigReferences.GetSnatch(handedness);
            if (grip) {
                grip.ForceDetach(hand);
            }

            RigReferences.RemoveJoint(handedness);
            RigReferences.SetGrabPoint(handedness, null);
        }

        public void OnHandUpdate(Hand hand) {
            switch (hand.handedness) {
                case Handedness.RIGHT:
                    if (serializedRightHand != null)
                        serializedRightHand.CopyTo(hand.Controller);
                    break;
                case Handedness.LEFT:
                    if (serializedLeftHand != null)
                        serializedLeftHand.CopyTo(hand.Controller);
                    break;
            }
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
            }
        }

        public void SwapAvatar(SerializedAvatarStats stats, string barcode) {
            avatarStats = stats;
            avatarId = barcode;
            _isAvatarDirty = true;
        }

        public void SetRagdoll(bool isRagdolled) {
            _ragdollState = isRagdolled;
            _isRagdollDirty = true;
        }

        public void SetSettings(SerializedPlayerSettings settings) {
            playerSettings = settings;
            _isSettingsDirty = true;
        }

        private void OnSwapAvatar(bool success) {
            // TODO: implement scaled poly blank if failure
            var rm = RigReferences.RigManager;

            if (!success) {
                rm.SwapAvatarCrate(PlayerRepUtilities.PolyBlankBarcode, false, (Action<bool>)OnSwapFallback);
            }
            // Update transforms
            else {
                UpdateNametagSettings();
            }
        }

        private void OnSwapFallback(bool success) {
            UpdateNametagSettings();
        }

        public void PlayPullCordEffects() {
            pullCord.PlayAvatarParticleEffects();
            pullCord.PlayClip(pullCord.switchAvatar, pullCord.ap3, pullCord.switchVolume, 4f, false);
        }

        public void SetVitals(SerializedBodyVitals vitals) {
            this.vitals = vitals;
            _isVitalsDirty = true;
        }

        public void CreateRep() {
            // Make sure we don't have any extra objects
            DestroyRep();

            CreateNametag();

            PlayerRepUtilities.CreateNewRig(OnRigCreated);
        }

        private void CreateNametag() {
            repCanvas = new GameObject("RepCanvas");
            repCanvasComponent = repCanvas.AddComponent<Canvas>();

            repCanvasComponent.renderMode = RenderMode.WorldSpace;
            repCanvasTransform = repCanvas.transform;
            repCanvasTransform.localScale = Vector3.one / NameTagDivider;

            repNameText = repCanvas.AddComponent<TextMeshProUGUI>();

            repNameText.alignment = TextAlignmentOptions.Midline;
            repNameText.enableAutoSizing = true;

            repNameText.text = Username;
            repNameText.font = PersistentAssetCreator.Font;
        }

        private void UpdateNametagSettings() {
            var rm = RigReferences.RigManager;
            if (!rm.IsNOC() && rm.avatar) {
                float height = rm.avatar.height / 1.76f;
                repCanvasTransform.localScale = Vector3.one / NameTagDivider * height;

                repNameText.text = Username;
            }
        }

        public void OnRigCreated(RigManager rig) {
            pullCord = rig.GetComponentInChildren<PullCordDevice>(true);

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
            pelvisPDController.OnResetDerivatives(repPelvis.transform);

            repFootBall = rig.physicsRig.physG.GetComponent<Rigidbody>();
            footPDController.OnResetDerivatives(repFootBall.transform);

            repControllerRig = rig.openControllerRig;
            repPlayspace = rig.openControllerRig.vrRoot.transform;

            repLeftController = repControllerRig.leftController;
            repRightController = repControllerRig.rightController;

            RigReferences = new RigReferenceCollection(rig);

            PlayerRepUtilities.FillTransformArray(ref repTransforms, rig);
            PlayerRepUtilities.FillGameworldArray(ref gameworldRigTransforms, rig);

            // Make sure the rig gets its initial avatar and settings
            MarkDirty();
        }

        public void MarkDirty() {
            _isAvatarDirty = true;
            _isVitalsDirty = true;

            _isSettingsDirty = true;
            _isServerDirty = true;

            _isRagdollDirty = true;
            _ragdollState = false;
        }

        public static void OnRecreateReps() {
            foreach (var rep in Representations.Values) {
                rep.StartRepCreation();
            }
        }

        public void StartRepCreation() {
            MelonCoroutines.Start(Co_DelayCreateRep());
        }

        private IEnumerator Co_DelayCreateRep() {
            // Wait for loading
            while (LevelWarehouseUtilities.IsDelayedLoading() || PlayerId.IsLoading) {
                if (LevelWarehouseUtilities.IsLoading())
                    yield break;

                yield return null;
            }

            CreateRep();
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

                    var pos = serializedGameworldLocalTransforms[i].position.Expand();
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
                var rm = RigReferences.RigManager;
                var physHead = rm.physicsRig.m_head;
                repCanvasTransform.position = physHead.position + Vector3.up * NametagHeight * RigReferences.RigManager.avatar.height;

                if (!RigData.RigReferences.RigManager.IsNOC()) {
                    var head = RigData.RigReferences.RigManager.physicsRig.m_head;
                    repCanvasTransform.rotation = Quaternion.LookRotation(Vector3.Normalize(repCanvasTransform.position - head.position), head.up);
                }
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

                    repTransforms[i].localPosition = serializedLocalTransforms[i].position.Expand();
                    repTransforms[i].localRotation = serializedLocalTransforms[i].rotation.Expand();
                }

                var rm = RigReferences.RigManager;
                var controllerRig = rm.openControllerRig;

                controllerRig.feetOffset = serializedFeetOffset;
                controllerRig._crouchTarget = serializedCrouchTarget;
                controllerRig._spineCrouchOff = serializedSpineCrouchOff;

                rm.virtualHeptaRig.spineCrouchOffset = serializedSpineCrouchOff;

                controllerRig.travState = serializedTravState;
                controllerRig.vertState = serializedVertState;
                controllerRig.vrVertState = serializedVrVertState;
            }
            catch {
                // Literally no reason this should happen but it does
                // Doesn't cause anything soooo
            }
        }

        public void OnPelvisPin() {
            try {
                // Stop pelvis
                if (repPelvis.IsNOC() || serializedPelvis == null || serializedFootball == null)
                    return;

                // Move position with prediction
                if (Time.timeSinceLevelLoad - timeSincePelvisSent <= 1.5f) {
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
                if (SafetyUtilities.IsValidTime && !rigManager.activeSeat)
                {
                    var pos = serializedPelvis.position;
                    var rot = serializedPelvis.rotation.Expand();

                    repPelvis.AddForce(pelvisPDController.GetForce(repPelvis, repPelvis.transform, pos, predictVelocity), ForceMode.Acceleration);
                    repFootBall.AddForce(footPDController.GetForce(repFootBall, repFootBall.transform, serializedFootball.position, predictVelocity), ForceMode.Acceleration);

                    // We only want to apply angular force when ragdolled
                    if (rigManager.physicsRig.torso.spineInternalMult <= 0f)
                    {
                        repPelvis.AddTorque(pelvisPDController.GetTorque(repPelvis, repPelvis.transform, rot), ForceMode.Acceleration);
                        repFootBall.AddTorque(footPDController.GetTorque(repFootBall, repFootBall.transform, serializedFootball.rotation.Expand()), ForceMode.Acceleration);
                    }
                    else
                    {
                        pelvisPDController.OnResetRotDerivatives(repPelvis.transform);
                        footPDController.OnResetRotDerivatives(repFootBall.transform);
                    }
                }
                else {
                    pelvisPDController.OnResetDerivatives(repPelvis.transform);
                    footPDController.OnResetDerivatives(repFootBall.transform);
                }

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

                        // Zero our teleport velocity, cause the rig doesn't seem to do that on its own?
                        foreach (var rb in RigReferences.RigManager.physicsRig.GetComponentsInChildren<Rigidbody>()) {
                            rb.velocity = Vector3.zero;
                            rb.angularVelocity = Vector3.zero;
                        }

                        pelvisPDController.OnResetDerivatives(repPelvis.transform);
                        footPDController.OnResetDerivatives(repFootBall.transform);

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
                    using (var data = PlayerRepTransformData.Create(PlayerIdManager.LocalSmallId, syncedPoints, syncedPelvis, syncedFootball, syncedPlayspace, syncedLeftController, syncedRightController)) {
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

        private void OnRepUpdate() {
            if (!RigReferences.RigManager.IsNOC()) {
                OnHandUpdate(RigReferences.LeftHand);
                OnHandUpdate(RigReferences.RightHand);
            }
        }

        private void OnRepFixedUpdate() {
            OnPelvisPin();

            OnHandFixedUpdate(RigReferences.LeftHand);
            OnHandFixedUpdate(RigReferences.RightHand);

            if (RigReferences.RigManager.IsNOC() || !RigReferences.RigManager.activeSeat) {
                for (var i = 0; i < serializedGameworldLocalTransforms.Length; i++)
                    serializedGameworldLocalTransforms[i] = null;
            }
        }

        private void OnRepLateUpdate() {
            OnUpdateNametags();

            // Update the player if its dirty
            var rm = RigReferences.RigManager;
            if (!rm.IsNOC() && !rm._avatar.IsNOC()) {
                // Swap the avatar
                if (_isAvatarDirty) {
                    rm.SwapAvatarCrate(avatarId, false, (Action<bool>)OnSwapAvatar);
                    _isAvatarDirty = false;
                }

                // Change body vitals
                if (_isVitalsDirty) {
                    if (vitals != null) {
                        vitals.CopyTo(rm.bodyVitals);
                    }

                    _isVitalsDirty = false;
                }
                
                // Toggle ragdoll mode
                if (_isRagdollDirty) {
                    if (_ragdollState)
                        rm.physicsRig.RagdollRig();
                    else
                        rm.physicsRig.UnRagdollRig();

                    _isRagdollDirty = false;
                }

                // Update settings
                if (_isSettingsDirty) {
                    if (playerSettings != null) {
                        repNameText.color = playerSettings.nametagColor;
                    }

                    _isSettingsDirty = false;
                }
                
                // Update server side settings
                if (_isServerDirty) {
                    repCanvas.gameObject.SetActive(FusionPreferences.ShowNametags);

                    _isServerDirty = false;
                }
            }
            // Reset synced positions
            else {
                serializedPelvis = null;
                serializedFootball = null;
            }
        }

        public static void OnUpdate() {
            foreach (var rep in Representations.Values)
                rep.OnRepUpdate();
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

            FusionPreferences.OnServerSettingsChange -= OnServerSettingsChange;

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
            syncedFootball = RigData.RigReferences.RigManager.physicsRig.physG.transform;
            syncedPlayspace = RigData.RigReferences.RigManager.openControllerRig.vrRoot.transform;
            syncedLeftController = RigData.RigReferences.RigManager.openControllerRig.leftController;
            syncedRightController = RigData.RigReferences.RigManager.openControllerRig.rightController;

            PlayerRepUtilities.FillTransformArray(ref syncedPoints, RigData.RigReferences.RigManager);
            PlayerRepUtilities.FillGameworldArray(ref gameworldPoints, RigData.RigReferences.RigManager);
        }
    }
}
