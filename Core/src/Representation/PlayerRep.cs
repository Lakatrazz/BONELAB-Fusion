using LabFusion.Data;
using LabFusion.Extensions;
using LabFusion.Network;
using LabFusion.Utilities;
using LabFusion.Preferences;

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
        public const float NametagHeight = 0.23f;
        public const float NameTagDivider = 250f;

        public PlayerId PlayerId { get; private set; }
        public string Username { get; private set; } = "Unknown";

        public RigReferenceCollection RigReferences { get; private set; } = new RigReferenceCollection();

        /// <summary>
        /// Returns true if the transforms of the rep have been created yet.
        /// </summary>
        public bool IsCreated => RigReferences.IsValid;

        public static Transform[] syncedPoints = null;
        public static Transform[] gameworldPoints = null;
        public static Transform syncedPlayspace;
        public static Transform syncedPelvis;
        public static Hand syncedLeftHand;
        public static Hand syncedRightHand;

        public SerializedLocalTransform[] serializedLocalTransforms = new SerializedLocalTransform[RigAbstractor.TransformSyncCount];
        public SerializedLocalTransform[] serializedGameworldLocalTransforms = new SerializedLocalTransform[RigAbstractor.GameworldRigTransformCount];
        public SerializedTransform serializedPelvis;

        public float serializedFeetOffset;
        public float serializedCrouchTarget;
        public float serializedSpineCrouchOff;

        public float serializedVrTwist;

        public ControllerRig.TraversalState serializedTravState;
        public ControllerRig.VertState serializedVertState;
        public ControllerRig.VrVertState serializedVrVertState;

        public Vector3 predictVelocity;
        public Vector3 predictAngularVelocity;

        public PDController pelvisPDController;
        public float timeSincePelvisSent;

        public Transform[] repTransforms = new Transform[RigAbstractor.TransformSyncCount];
        public Transform[] gameworldRigTransforms = new Transform[RigAbstractor.GameworldRigTransformCount];

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
        public SerializedAvatarStats avatarStats = null;
        public SerializedPlayerSettings playerSettings = null;
        public string avatarId = CommonBarcodes.INVALID_AVATAR_BARCODE;

        public SerializedHand serializedLeftHand = null;
        public SerializedHand serializedRightHand = null;

        private bool _hasLockedPosition = false;

        private bool _isAvatarDirty = false;
        private bool _isVitalsDirty = false;

        private bool _isRagdollDirty = false;
        private bool _ragdollState = false;

        private bool _isSettingsDirty = false;
        private bool _isServerDirty = false;

        // Voice chat integration
        private const float _voiceUpdateStep = 0.3f;

        private AudioSource _voiceSource = null;

        private float _voiceUpdateTime = 0f;

        private float _voiceLoudness = 0f;
        private float _targetLoudness = 0f;

        private const float _sinAmplitude = 5f;
        private const float _sinOmega = 10f;

        public PlayerRep(PlayerId playerId)
        {
            // Store our ID
            PlayerId = playerId;

            // Hook into metadata changes
            playerId.OnMetadataChanged += OnMetadataChanged;
            OnMetadataChanged(playerId);

            // Insert the PlayerRep into the global list
            PlayerRepManager.Internal_InsertPlayerRep(this);

            pelvisPDController = new PDController();

            MultiplayerHooking.OnServerSettingsChanged += OnServerSettingsChanged;
            FusionOverrides.OnOverridesChanged += OnOverridesChanged;

            ResetSerializedTransforms();

            StartRepCreation();
        }

        public void InsertVoiceSource(AudioSource source) {
            _voiceSource = source;
        }

        public float GetVoiceLoudness() => _voiceLoudness;

        private void OnUpdateVoiceSource() {
            if (_voiceSource.IsNOC())
                return;

            // Update the amplitude
            _voiceUpdateTime += Time.deltaTime;
            if (_voiceUpdateTime >= _voiceUpdateStep) {
                _voiceUpdateTime = 0f;

                var spectrum = _voiceSource.GetSpectrumData(256, 0, FFTWindow.Rectangular);

                float gain = 0f;
                for (var i = 0; i < spectrum.Length; i++) {
                    gain += Mathf.Abs(spectrum[i]);
                }

                if (spectrum.Length > 0)
                    gain /= (float)spectrum.Length;

                _targetLoudness = gain;

                // Add affectors
                _targetLoudness *= 100f;
                _targetLoudness = Mathf.Clamp(_targetLoudness, 0f, 2f);
            }

            // Lerp towards the desired value
            float sin = Mathf.Abs(_sinAmplitude * Mathf.Sin(_sinOmega * Time.timeSinceLevelLoad));
            sin = Mathf.Clamp01(sin);

            _voiceLoudness = Mathf.Lerp(_voiceLoudness * sin, _targetLoudness, Time.deltaTime * 12f);

            // Modify the source settings
            var rm = RigReferences.RigManager;
            if (IsCreated && rm._avatar) {
                float heightMult = rm._avatar.height / 1.76f;

                _voiceSource.spatialBlend = 1f;
                _voiceSource.minDistance = 0.5f * heightMult;
                _voiceSource.maxDistance = 30f * heightMult;
                _voiceSource.reverbZoneMix = Mathf.Clamp(0.35f * heightMult, 0f, 1.02f);
                _voiceSource.dopplerLevel = 0.5f;

                var mouthSource = rm.physicsRig.headSfx.mouthSrc;
                _voiceSource.transform.position = mouthSource.transform.position;

                // Set the mixer
                if (_voiceSource.outputAudioMixerGroup == null && !pullCord.IsNOC())
                    _voiceSource.outputAudioMixerGroup = pullCord.mixerGroup;
            }
            else {
                _voiceSource.spatialBlend = 0f;
                _voiceSource.minDistance = 0.5f;
                _voiceSource.maxDistance = 30f;
                _voiceSource.reverbZoneMix = 0.35f;
                _voiceSource.dopplerLevel = 0.5f;
            }
        }

        private void OnMetadataChanged(PlayerId id) {
            // Read display name
            if (id.TryGetDisplayName(out var name)) {
                Username = name;
            }

            UpdateNametagSettings();
        }

        private void OnOverridesChanged() {
            _isServerDirty = true;
        }

        public void ResetSerializedTransforms() {
            for (var i = 0; i < RigAbstractor.TransformSyncCount; i++) {
                serializedLocalTransforms[i] = new SerializedLocalTransform(Vector3Extensions.zero, Quaternion.identity);
            }
        }

        private void OnServerSettingsChanged() {
            _isServerDirty = true;

            OnMetadataChanged(PlayerId);
        }

        public void AttachObject(Handedness handedness, Grip grip, SimpleTransform? targetInBase = null) {
            var hand = RigReferences.GetHand(handedness);
            if (hand == null)
                return;

            if (grip) {
                // Detach existing grip
                hand.TryDetach();

                // Check if the grip can be interacted with
                if (grip.IsInteractionDisabled || (grip.HasHost && grip.Host.IsInteractionDisabled))
                    return;

                // Attach the hand
                grip.TryAttach(hand, grip.CheckInstantAttach(), targetInBase);
            }
        }

        public void DetachObject(Handedness handedness) {
            var hand = RigReferences.GetHand(handedness);
            if (hand == null)
                return;

            hand.TryDetach();
        }

        public void OnHandUpdate(Hand hand) {
            switch (hand.handedness) {
                case Handedness.RIGHT:
                    if (serializedRightHand != null)
                        serializedRightHand.CopyTo(hand, hand.Controller);
                    break;
                case Handedness.LEFT:
                    if (serializedLeftHand != null)
                        serializedLeftHand.CopyTo(hand, hand.Controller);
                    break;
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
            var rm = RigReferences.RigManager;

            if (!success) {
                rm.SwapAvatarCrate(PlayerRepUtilities.PolyBlankBarcode, true, (Action<bool>)OnSwapFallback);
            }
            else {
                UpdateNametagSettings();
            }
        }

        private void OnSwapFallback(bool success) {
            UpdateNametagSettings();
        }

        public void PlayPullCordEffects() {
            if (!IsCreated)
                return;

            pullCord.PlayAvatarParticleEffects();
            pullCord.PlayClip(pullCord.switchAvatar, pullCord.ap3, pullCord.switchVolume, 4f, false);
        }

        public void SetBallEnabled(bool isEnabled) {
            if (!IsCreated)
                return;

            // If the ball should be enabled, make the distance required infinity so it always shows
            if (isEnabled) {
                pullCord.handShowDist = float.PositiveInfinity;
            }
            // If it should be disabled, make the distance zero so that it disables itself
            else {
                pullCord.handShowDist = 0f;
            }
        }

        public void SetVitals(SerializedBodyVitals vitals) {
            this.vitals = vitals;
            _isVitalsDirty = true;
        }

        private void CreateRep() {
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
            repCanvasTransform.localScale = Vector3Extensions.one / NameTagDivider;

            repNameText = repCanvas.AddComponent<TextMeshProUGUI>();

            repNameText.alignment = TextAlignmentOptions.Midline;
            repNameText.enableAutoSizing = true;

            repNameText.text = Username;
            repNameText.font = PersistentAssetCreator.Font;
        }

        public float GetNametagOffset() {
            float offset = NametagHeight;

            var rm = RigReferences.RigManager;
            if (IsCreated && rm._avatar)
                offset *= rm._avatar.height;

            return offset;
        }

        private void UpdateNametagSettings() {
            var rm = RigReferences.RigManager;
            if (IsCreated && rm.avatar) {
                float height = rm.avatar.height / 1.76f;
                repCanvasTransform.localScale = Vector3Extensions.one / NameTagDivider * height;

                repNameText.text = Username;
            }
        }

        public void OnRigCreated(RigManager rig) {
            // Get the pull cord and prevent it from enabling
            pullCord = rig.GetComponentInChildren<PullCordDevice>(true);

            SetBallEnabled(false);

            // Swap the open controllers for generic controllers
            // Left hand
            var leftHaptor = rig.openControllerRig.leftController.haptor;
            rig.openControllerRig.leftController = rig.openControllerRig.leftController.gameObject.AddComponent<Controller>();
            rig.openControllerRig.leftController.manager = rig.openControllerRig;
            leftHaptor.device_Controller = rig.openControllerRig.leftController;
            rig.openControllerRig.leftController.handedness = Handedness.LEFT;

            // Right hand
            var rightHaptor = rig.openControllerRig.rightController.haptor;
            rig.openControllerRig.rightController = rig.openControllerRig.rightController.gameObject.AddComponent<Controller>();
            rig.openControllerRig.rightController.manager = rig.openControllerRig;
            rightHaptor.device_Controller = rig.openControllerRig.rightController;
            rig.openControllerRig.rightController.handedness = Handedness.RIGHT;

            // Insert the connection between the rig manager and player rep so we can find this
            PlayerRepManager.Internal_AddRigManager(rig, this);

            // Store all of the necessary rig references
            repPelvis = rig.physicsRig.m_pelvis.GetComponent<Rigidbody>();

            repControllerRig = rig.openControllerRig;
            repPlayspace = rig.GetSmoothTurnTransform();

            repLeftController = repControllerRig.leftController;
            repRightController = repControllerRig.rightController;

            RigReferences = new RigReferenceCollection(rig);

            // Shrink holster hitboxes for easier grabbing
            foreach (var slot in RigReferences.RigSlots) {
                foreach (var box in slot.GetComponentsInChildren<BoxCollider>()) {
                    // Only affect trigger colliders just incase
                    if (box.isTrigger)
                        box.size *= 0.4f;
                }
            }

            // Get the synced transform arrays so we can set tracked positions later
            RigAbstractor.FillTransformArray(ref repTransforms, rig);
            RigAbstractor.FillGameworldArray(ref gameworldRigTransforms, rig);

            // Make sure the rig gets its initial avatar and settings
            MarkDirty();

            // Invoke the hook
            MultiplayerHooking.Internal_OnPlayerRepCreated(rig);
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
            for (var i = 0; i < PlayerRepManager.PlayerReps.Count; i++) {
                PlayerRepManager.PlayerReps[i].StartRepCreation();
            }
        }

        public void StartRepCreation() {
            MelonCoroutines.Start(Co_DelayCreateRep());
        }

        private IEnumerator Co_DelayCreateRep() {
            // Delay some extra time
            for (var i = 0; i < 120; i++) {
                if (FusionSceneManager.IsLoading())
                    yield break;

                yield return null;
            }

            // Wait for loading
            while (FusionSceneManager.IsDelayedLoading() || PlayerId.GetMetadata(MetadataHelper.LoadingKey) == bool.TrueString) {
                if (FusionSceneManager.IsLoading())
                    yield break;

                yield return null;
            }

            // Make sure the rep still exists
            if (PlayerId == null || !PlayerId.IsValid)
                yield break;

            CreateRep();
        }

        public void OnHeptaBody2Update() {
            try {
                if (!IsCreated)
                    return;

                for (var i = 0; i < RigAbstractor.GameworldRigTransformCount; i++)
                {
                    if (serializedGameworldLocalTransforms[i] == null)
                        break;

                    var pos = serializedGameworldLocalTransforms[i].position;
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
            // Update nametag
            var rm = RigReferences.RigManager;

            if (IsCreated) {
                var physHead = rm.physicsRig.m_head;
                repCanvasTransform.position = physHead.position + Vector3Extensions.up * GetNametagOffset();
                repCanvasTransform.LookAtPlayer();
            }
        }

        public void OnControllerRigUpdate() {
            try
            {
                if (!IsCreated)
                    return;

                if (serializedLocalTransforms == null)
                    return;

                for (var i = 0; i < RigAbstractor.TransformSyncCount; i++)
                {
                    repTransforms[i].localPosition = serializedLocalTransforms[i].position;
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
                if (!IsCreated || serializedPelvis == null)
                    return;

                // Check for seating
                var rigManager = RigReferences.RigManager;

                if (rigManager.activeSeat) {
                    return;
                }

                Transform pelvisTransform = repPelvis.transform;
                Vector3 pelvisPosition = pelvisTransform.position;
                Quaternion pelvisRotation = pelvisTransform.rotation;

                // Move position with prediction
                if (Time.realtimeSinceStartup - timeSincePelvisSent <= 1.5f) {
                    serializedPelvis.position += predictVelocity * Time.fixedDeltaTime;

                    _hasLockedPosition = false;
                }
                else if (!_hasLockedPosition) {
                    serializedPelvis.position = pelvisPosition;
                    predictVelocity = Vector3Extensions.zero;
                    predictAngularVelocity = Vector3Extensions.zero;

                    _hasLockedPosition = true;
                }

                // Apply velocity
                if (SafetyUtilities.IsValidTime)
                {
                    var pos = serializedPelvis.position;
                    var rot = serializedPelvis.rotation.Expand();

                    repPelvis.AddForce(pelvisPDController.GetForce(repPelvis, pelvisPosition, repPelvis.velocity, pos, predictVelocity), ForceMode.Acceleration);
                    // We only want to apply angular force when ragdolled
                    if (rigManager.physicsRig.torso.spineInternalMult <= 0f) {
                        repPelvis.AddTorque(pelvisPDController.GetTorque(repPelvis, pelvisRotation, repPelvis.angularVelocity, rot, predictAngularVelocity), ForceMode.Acceleration);
                    }
                }

                // Check for stability teleport
                float distSqr = (pelvisPosition - serializedPelvis.position).sqrMagnitude;
                if (distSqr > (2f * (Vector3Extensions.GetMagnitude(predictVelocity) + 1f))) {
                    // Get teleport position
                    var pos = serializedPelvis.position;
                    var physRig = RigReferences.RigManager.physicsRig;

                    // Offset
                    pos += physRig.feet.transform.position - physRig.m_pelvis.position;
                    pos += physRig.footballRadius * -physRig.m_pelvis.up;

                    RigReferences.RigManager.Teleport(pos);

                    // Zero our teleport velocity, cause the rig doesn't seem to do that on its own?
                    foreach (var rb in RigReferences.RigManager.physicsRig.GetComponentsInChildren<Rigidbody>()) {
                        rb.velocity = Vector3Extensions.zero;
                        rb.angularVelocity = Vector3Extensions.zero;
                    }

                    // Reset locosphere and knee pos so the rig doesn't get stuck
                    physRig.knee.transform.position = serializedPelvis.position;
                    physRig.feet.transform.position = serializedPelvis.position;
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

                using (var writer = FusionWriter.Create(PlayerRepGameworldData.Size))
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

                using (var writer = FusionWriter.Create(PlayerRepTransformData.Size)) {
                    using (var data = PlayerRepTransformData.Create(PlayerIdManager.LocalSmallId, syncedPoints, syncedPelvis, syncedPlayspace, syncedLeftHand, syncedRightHand)) {
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
            if (NetworkInfo.HasServer && RigData.HasPlayer) {
                if (!TrySendRep())
                    OnCachePlayerTransforms();
                else if (RigData.RigReferences.RigManager.activeSeat) {
                    TrySendGameworldRep();
                }
            }
            else {
                syncedPoints = null;
                gameworldPoints = null;
            }
        }

        private void OnRepUpdate() {
            if (!IsCreated)
                return;

            OnHandUpdate(RigReferences.LeftHand);
            OnHandUpdate(RigReferences.RightHand);

            OnUpdateVoiceSource();
        }

        private void OnRepFixedUpdate() {
            if (!IsCreated || !RigReferences.RigManager.activeSeat) {
                // Remove old values from the gameworld transforms
                for (var i = 0; i < serializedGameworldLocalTransforms.Length; i++)
                    serializedGameworldLocalTransforms[i] = null;

                // Only return if the first argument is true
                if (!IsCreated)
                    return;
            }

            OnPelvisPin();
        }

        public void DetachRepGrips() {
            foreach (var grip in RigReferences.RigGrips) {
                foreach (var hand in grip.attachedHands.ToArray()) {
                    if (hand.manager == RigData.RigReferences.RigManager)
                        grip.TryDetach(hand);
                }
            }
        }

        private void OnRepLateUpdate() {
            if (!IsCreated) {
                serializedPelvis = null;
                return;
            }

            OnUpdateNametags();

            // Update the player if its dirty and has an avatar
            var rm = RigReferences.RigManager;
            if (!rm._avatar.IsNOC()) {
                // Swap the avatar
                if (_isAvatarDirty) {
                    rm.SwapAvatarCrate(avatarId, true, (Action<bool>)OnSwapAvatar);
                    _isAvatarDirty = false;

                    PlayerAdditionsHelper.OnAvatarChanged(rm);
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
                    repCanvas.gameObject.SetActive(FusionPreferences.NametagsEnabled && FusionOverrides.ValidateNametag(PlayerId));

                    _isServerDirty = false;
                }
            }
        }

        public static void OnUpdate() {
            for (var i = 0; i < PlayerRepManager.PlayerReps.Count; i++)
                PlayerRepManager.PlayerReps[i].OnRepUpdate();
        }

        public static void OnFixedUpdate() {
            for (var i = 0; i < PlayerRepManager.PlayerReps.Count; i++)
                PlayerRepManager.PlayerReps[i].OnRepFixedUpdate();
        }

        public static void OnLateUpdate()
        {
            for (var i = 0; i < PlayerRepManager.PlayerReps.Count; i++)
                PlayerRepManager.PlayerReps[i].OnRepLateUpdate();
        }

        /// <summary>
        /// Destroys anything about the PlayerRep and frees it from memory.
        /// </summary>
        public void Dispose() {
            PlayerRepManager.Internal_RemovePlayerRep(this);

            DestroyRep();

            GC.SuppressFinalize(this);

            MultiplayerHooking.OnServerSettingsChanged -= OnServerSettingsChanged;

#if DEBUG
            FusionLogger.Log($"Disposed PlayerRep with small id {PlayerId.SmallId}");
#endif
        }

        /// <summary>
        /// Destroys the GameObjects of the PlayerRep. Does not free it from memory or remove it from its slots. Use Dispose for that.
        /// </summary>
        public void DestroyRep() {
            if (IsCreated)
                GameObject.Destroy(RigReferences.RigManager.gameObject);

            if (!repCanvas.IsNOC())
                GameObject.Destroy(repCanvas.gameObject);
        }

        public static void OnCachePlayerTransforms() {
            if (!RigData.HasPlayer)
                return;

            var rm = RigData.RigReferences.RigManager;
            syncedPelvis = rm.physicsRig.m_pelvis;
            syncedPlayspace = rm.GetSmoothTurnTransform();
            syncedLeftHand = rm.physicsRig.leftHand;
            syncedRightHand = rm.physicsRig.rightHand;

            RigAbstractor.FillTransformArray(ref syncedPoints, rm);
            RigAbstractor.FillGameworldArray(ref gameworldPoints, rm);
        }
    }
}
