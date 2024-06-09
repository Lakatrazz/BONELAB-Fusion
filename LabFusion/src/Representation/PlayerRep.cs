using LabFusion.Data;
using LabFusion.Extensions;
using LabFusion.Network;
using LabFusion.Utilities;
using LabFusion.Preferences;
using Il2CppSLZ.Interaction;
using Il2CppSLZ.Rig;
using Il2CppSLZ.Marrow.Utilities;
using System.Collections;

using Il2CppTMPro;

using UnityEngine;

using MelonLoader;
using LabFusion.Voice;
using Il2CppSLZ.Marrow.Interaction;
using Il2CppSLZ.Bonelab;

namespace LabFusion.Representation
{
    public class PlayerRep
    {
        public const float NametagHeight = 0.23f;
        public const float NameTagDivider = 250f;

        public PlayerId PlayerId { get; private set; }
        public string Username { get; private set; } = "Unknown";

        public RigReferenceCollection RigReferences { get; private set; } = new RigReferenceCollection();

        /// <summary>
        /// Returns true if the transforms of the rep have been created yet.
        /// </summary>
        public bool IsCreated => RigReferences.IsValid;

        /// <summary>
        /// The distance of this PlayerRep's head to the local player's head (squared).
        /// </summary>
        public float DistanceSqr { get; private set; }

        /// <summary>
        /// Whether or not the player's microphone logic is disabled.
        /// </summary>
        public bool MicrophoneDisabled { get; private set; }

        public SerializedLocalTransform[] serializedLocalTransforms = new SerializedLocalTransform[RigAbstractor.TransformSyncCount];
        public SerializedTransform serializedPelvis;

        public Vector3 predictVelocity;
        public Vector3 predictAngularVelocity;

        public PDController pelvisPDController;
        public float timeSincePelvisSent;

        public Transform[] repTransforms = new Transform[RigAbstractor.TransformSyncCount];

        public ControllerRig repControllerRig;
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

        private bool _isQuestUser = false;

        // Voice chat integration
        private float _maxMicrophoneDistance = 30f;

        private IVoiceSpeaker _speaker = null;
        private AudioSource _voiceSource = null;
        private bool _hasVoice = false;

        private bool _spatialized = false;

        private readonly JawFlapper _flapper = new();

        public JawFlapper JawFlapper => _flapper;

        public PlayerRep(PlayerId playerId)
        {
            // Store our ID
            PlayerId = playerId;

            OnMetadataChanged(playerId);

            // Hook important methods
            HookMethods();

            _isQuestUser = PlayerId.GetMetadata(MetadataHelper.PlatformKey) == "QUEST";

            pelvisPDController = new PDController();

            ResetSerializedTransforms();

            StartRepCreation();
        }

        private void HookMethods()
        {
            PlayerId.OnMetadataChanged += OnMetadataChanged;
            PlayerId.OnDestroyedEvent += Cleanup;

            PlayerRepManager.Internal_InsertPlayerRep(this);

            MultiplayerHooking.OnServerSettingsChanged += OnServerSettingsChanged;
            FusionOverrides.OnOverridesChanged += OnOverridesChanged;
        }

        private void UnhookMethods()
        {
            PlayerId.OnMetadataChanged -= OnMetadataChanged;
            PlayerId.OnDestroyedEvent -= Cleanup;

            PlayerRepManager.Internal_RemovePlayerRep(this);

            MultiplayerHooking.OnServerSettingsChanged -= OnServerSettingsChanged;
            FusionOverrides.OnOverridesChanged -= OnOverridesChanged;
        }

        public void InsertVoiceSource(IVoiceSpeaker speaker, AudioSource source)
        {
            _speaker = speaker;
            _voiceSource = source;
            _hasVoice = true;
        }

        private void OnUpdateVoiceSource()
        {
            if (!_hasVoice)
                return;

            // Modify the source settings
            var rm = RigReferences.RigManager;
            if (IsCreated)
            {
                var mouthSource = rm.physicsRig.headSfx.mouthSrc;
                _voiceSource.transform.position = mouthSource.transform.position;

                if (!_spatialized)
                {
                    UpdateVoiceSourceSettings();
                    _spatialized = true;
                }

                MicrophoneDisabled = DistanceSqr > (_maxMicrophoneDistance * _maxMicrophoneDistance) * 1.2f;
            }
            else
            {
                _voiceSource.spatialBlend = 0f;
                _voiceSource.minDistance = 0f;
                _voiceSource.maxDistance = 30f;
                _voiceSource.reverbZoneMix = 0f;
                _voiceSource.dopplerLevel = 0.5f;

                _spatialized = false;
                MicrophoneDisabled = false;
            }

            // Update the jaw movement
            if (MicrophoneDisabled)
            {
                _flapper.ClearJaw();
            }
            else
            {
                _flapper.UpdateJaw(_speaker.GetVoiceAmplitude());
            }
        }

        private void OnMetadataChanged(PlayerId id)
        {
            // Read display name
            if (id.TryGetDisplayName(out var name))
            {
                Username = name;
            }

            UpdateNametagSettings();
        }

        private void OnOverridesChanged()
        {
            _isServerDirty = true;
        }

        public void ResetSerializedTransforms()
        {
            for (var i = 0; i < RigAbstractor.TransformSyncCount; i++)
            {
                serializedLocalTransforms[i] = SerializedLocalTransform.Default;
            }
        }

        private void OnServerSettingsChanged()
        {
            _isServerDirty = true;

            OnMetadataChanged(PlayerId);
        }

        public void AttachObject(Handedness handedness, Grip grip, SimpleTransform? targetInBase = null)
        {
            var hand = RigReferences.GetHand(handedness);
            if (hand == null)
                return;

            if (grip)
            {
                // Detach existing grip
                hand.TryDetach();

                // Check if the grip can be interacted with
                if (grip.IsInteractionDisabled || (grip.HasHost && grip.Host.IsInteractionDisabled))
                    return;

                // Attach the hand
                grip.TryAttach(hand, false, targetInBase);
            }
        }

        public void DetachObject(Handedness handedness)
        {
            var hand = RigReferences.GetHand(handedness);
            if (hand == null)
                return;

            hand.TryDetach();
        }

        public void OnHandUpdate(Hand hand)
        {
            switch (hand.handedness)
            {
                case Handedness.RIGHT:
                    serializedRightHand?.CopyTo(hand, hand.Controller);
                    break;
                case Handedness.LEFT:
                    serializedLeftHand?.CopyTo(hand, hand.Controller);
                    break;
            }
        }

        public void SwapAvatar(SerializedAvatarStats stats, string barcode)
        {
            avatarStats = stats;
            avatarId = barcode;
            _isAvatarDirty = true;
        }

        public void SetRagdoll(bool isRagdolled)
        {
            _ragdollState = isRagdolled;
            _isRagdollDirty = true;
        }

        public void SetSettings(SerializedPlayerSettings settings)
        {
            playerSettings = settings;
            _isSettingsDirty = true;
        }

        private void OnSwapAvatar(bool success)
        {
            var rm = RigReferences.RigManager;

            if (!success)
            {
                RigReferences.SwapAvatarCrate(FusionAvatar.POLY_BLANK_BARCODE, OnSwapFallback, OnPrepareAvatar);
            }
            else
            {
                UpdateAvatarSettings();
            }
        }

        private void OnPrepareAvatar(string barcode, GameObject avatar)
        {
            // If we have synced avatar stats, set the scale properly
            if (avatarStats != null)
            {
                Transform transform = avatar.transform;

                // Polyblank should just scale based on the custom avatar height
                if (barcode == FusionAvatar.POLY_BLANK_BARCODE)
                {
                    float newHeight = avatarStats.height;
                    transform.localScale = Vector3Extensions.one * (newHeight / 1.76f);
                }
                // Otherwise, apply the synced scale
                else
                {
                    transform.localScale = avatarStats.localScale;
                }
            }
        }

        private void OnSwapFallback(bool success)
        {
            UpdateAvatarSettings();
        }

        internal void Internal_OnAvatarChanged(string barcode)
        {
            if (!FusionAvatar.IsMatchingAvatar(barcode, avatarId))
                _isAvatarDirty = true;
        }

        public void PlayPullCordEffects()
        {
            if (!IsCreated)
                return;

            pullCord.PlayAvatarParticleEffects();
            // pullCord.PlayClip(pullCord.switchAvatar, pullCord.ap3, pullCord.switchVolume, 4f, false);
        }

        public void SetBallEnabled(bool isEnabled)
        {
            if (!IsCreated)
                return;

            // If the ball should be enabled, make the distance required infinity so it always shows
            if (isEnabled)
            {
                pullCord.handShowDist = float.PositiveInfinity;
            }
            // If it should be disabled, make the distance zero so that it disables itself
            else
            {
                pullCord.handShowDist = 0f;
            }
        }

        public void SetVitals(SerializedBodyVitals vitals)
        {
            this.vitals = vitals;
            _isVitalsDirty = true;
        }

        private void CreateRep()
        {
            // Make sure we don't have any extra objects
            DestroyRep();

            CreateNametag();

            PlayerRepUtilities.CreateNewRig(OnRigCreated);
        }

        private void CreateNametag()
        {
            repCanvas = new GameObject("RepCanvas");
            repCanvasComponent = repCanvas.AddComponent<Canvas>();

            repCanvasComponent.renderMode = RenderMode.WorldSpace;
            repCanvasTransform = repCanvas.transform;
            repCanvasTransform.localScale = Vector3Extensions.one / NameTagDivider;

            repNameText = repCanvas.AddComponent<TextMeshProUGUI>();

            repNameText.alignment = TextAlignmentOptions.Midline;
            repNameText.enableAutoSizing = true;

            repNameText.text = Username;
            if (_isQuestUser)
            {
                repNameText.richText = true;
                repNameText.text += " <size=60%>Q";
            }
            repNameText.font = PersistentAssetCreator.Font;
        }

        public float GetNametagOffset()
        {
            float offset = NametagHeight;

            var rm = RigReferences.RigManager;
            if (IsCreated && rm._avatar)
                offset *= rm._avatar.height;

            return offset;
        }

        private void UpdateAvatarSettings()
        {
            UpdateNametagSettings();
            UpdateVoiceSourceSettings();
        }

        private void UpdateNametagSettings()
        {
            var rm = RigReferences.RigManager;
            if (IsCreated && rm.avatar)
            {
                float height = rm.avatar.height / 1.76f;
                repCanvasTransform.localScale = Vector3Extensions.one / NameTagDivider * height;

                repNameText.text = Username;
                if (_isQuestUser)
                {
                    repNameText.richText = true;
                    repNameText.text += " <size=60%>Q";
                }
            }
        }

        private void UpdateVoiceSourceSettings()
        {
            if (_voiceSource == null)
                return;

            var rm = RigReferences.RigManager;
            if (IsCreated && rm._avatar)
            {
                float heightMult = rm._avatar.height / 1.76f;

                _voiceSource.spatialBlend = 1f;
                _voiceSource.reverbZoneMix = 0.1f;

                _maxMicrophoneDistance = 30f * heightMult;

                _voiceSource.SetRealisticRolloff(0.5f, _maxMicrophoneDistance);

                // Set the mixer
                if (_voiceSource.outputAudioMixerGroup == null)
                    _voiceSource.outputAudioMixerGroup = pullCord._map3._mixerGroup;
            }
        }

        public void OnRigCreated(RigManager rig)
        {
            // Get the pull cord and prevent it from enabling
            pullCord = rig.GetComponentInChildren<PullCordDevice>(true);

            SetBallEnabled(false);

            // Swap the open controllers for generic controllers
            // Left hand
            var leftHaptor = rig.ControllerRig.leftController.haptor;
            rig.ControllerRig.leftController = rig.ControllerRig.leftController.gameObject.AddComponent<BaseController>();
            rig.ControllerRig.leftController.contRig = rig.ControllerRig;
            leftHaptor.device_Controller = rig.ControllerRig.leftController;
            rig.ControllerRig.leftController.handedness = Handedness.LEFT;

            // Right hand
            var rightHaptor = rig.ControllerRig.rightController.haptor;
            rig.ControllerRig.rightController = rig.ControllerRig.rightController.gameObject.AddComponent<BaseController>();
            rig.ControllerRig.rightController.contRig = rig.ControllerRig;
            rightHaptor.device_Controller = rig.ControllerRig.rightController;
            rig.ControllerRig.rightController.handedness = Handedness.RIGHT;

            // Insert the connection between the rig manager and player rep so we can find this
            PlayerRepManager.Internal_AddRigManager(rig, this);

            // Store all of the necessary rig references
            repPelvis = rig.physicsRig.m_pelvis.GetComponent<Rigidbody>();

            repControllerRig = rig.ControllerRig;
            repPlayspace = rig.GetSmoothTurnTransform();

            repLeftController = repControllerRig.leftController;
            repRightController = repControllerRig.rightController;

            RigReferences = new RigReferenceCollection(rig);

            // Shrink holster hitboxes for easier grabbing
            foreach (var slot in RigReferences.RigSlots)
            {
                foreach (var box in slot.GetComponentsInChildren<BoxCollider>())
                {
                    // Only affect trigger colliders just incase
                    if (box.isTrigger)
                        box.size *= 0.4f;
                }
            }

            // Get the synced transform arrays so we can set tracked positions later
            RigAbstractor.FillTransformArray(ref repTransforms, rig);

            // Make sure the rig gets its initial avatar and settings
            MarkDirty();

            // Invoke the hook
            MultiplayerHooking.Internal_OnPlayerRepCreated(rig);
        }

        public void MarkDirty()
        {
            _isAvatarDirty = true;
            _isVitalsDirty = true;

            _isSettingsDirty = true;
            _isServerDirty = true;

            _isRagdollDirty = true;
            _ragdollState = false;
        }

        public static void OnRecreateReps()
        {
            for (var i = 0; i < PlayerRepManager.PlayerReps.Count; i++)
            {
                PlayerRepManager.PlayerReps[i].StartRepCreation();
            }
        }

        public void StartRepCreation()
        {
            MelonCoroutines.Start(Co_DelayCreateRep());
        }

        private IEnumerator Co_DelayCreateRep()
        {
            // Delay some extra time
            for (var i = 0; i < 120; i++)
            {
                if (FusionSceneManager.IsLoading())
                    yield break;

                yield return null;
            }

            // Wait for loading
            while (FusionSceneManager.IsDelayedLoading() || PlayerId.GetMetadata(MetadataHelper.LoadingKey) == bool.TrueString)
            {
                if (FusionSceneManager.IsLoading())
                    yield break;

                yield return null;
            }

            // Make sure the rep still exists
            if (PlayerId == null || !PlayerId.IsValid)
                yield break;

            CreateRep();
        }

        public void OnUpdateNametags()
        {
            // Update nametag
            var rm = RigReferences.RigManager;

            if (IsCreated)
            {
                var physHead = rm.physicsRig.m_head;
                repCanvasTransform.position = physHead.position + Vector3Extensions.up * GetNametagOffset();
                repCanvasTransform.LookAtPlayer();
            }
        }

        public void OnControllerRigUpdate()
        {
            if (!IsCreated)
                return;

            for (var i = 0; i < RigAbstractor.TransformSyncCount; i++)
            {
                repTransforms[i].localPosition = serializedLocalTransforms[i].position;
                repTransforms[i].localRotation = serializedLocalTransforms[i].rotation;
            }
        }

        public void OnPelvisPin()
        {
            try
            {
                // Stop pelvis
                if (!IsCreated || serializedPelvis == null)
                {
                    pelvisPDController.Reset();
                    return;
                }

                // Check for seating
                var rigManager = RigReferences.RigManager;

                if (rigManager.activeSeat)
                {
                    pelvisPDController.Reset();
                    return;
                }

                Transform pelvisTransform = repPelvis.transform;
                Vector3 pelvisPosition = pelvisTransform.position;
                Quaternion pelvisRotation = pelvisTransform.rotation;

                // Move position with prediction
                if (TimeUtilities.TimeSinceStartup - timeSincePelvisSent <= 1.5f)
                {
                    serializedPelvis.position += predictVelocity * TimeUtilities.FixedDeltaTime;

                    _hasLockedPosition = false;
                }
                else if (!_hasLockedPosition)
                {
                    serializedPelvis.position = pelvisPosition;
                    predictVelocity = Vector3Extensions.zero;
                    predictAngularVelocity = Vector3Extensions.zero;

                    _hasLockedPosition = true;
                }

                // Apply velocity
                if (SafetyUtilities.IsValidTime)
                {
                    var pos = serializedPelvis.position;
                    var rot = serializedPelvis.rotation;

                    repPelvis.AddForce(pelvisPDController.GetForce(pelvisPosition, repPelvis.velocity, pos, predictVelocity), ForceMode.Acceleration);
                    
                    // We only want to apply angular force when ragdolled
                    if (rigManager.physicsRig.torso.spineInternalMult <= 0f)
                    {
                        repPelvis.AddTorque(pelvisPDController.GetTorque(pelvisRotation, repPelvis.angularVelocity, rot, predictAngularVelocity), ForceMode.Acceleration);
                    }
                    else
                        pelvisPDController.ResetRotation();
                }

                // Check for stability teleport
                float distSqr = (pelvisPosition - serializedPelvis.position).sqrMagnitude;
                if (distSqr > (2f * (predictVelocity.magnitude + 1f)))
                {
                    // Get teleport position
                    var pos = serializedPelvis.position;
                    var physRig = RigReferences.RigManager.physicsRig;

                    // Offset
                    pos += physRig.feet.transform.position - physRig.m_pelvis.position;
                    pos += physRig.footballRadius * -physRig.m_pelvis.up;

                    RigReferences.RigManager.Teleport(pos);

                    // Zero our teleport velocity, cause the rig doesn't seem to do that on its own?
                    foreach (var rb in RigReferences.RigManager.physicsRig.selfRbs)
                    {
                        rb.velocity = Vector3Extensions.zero;
                        rb.angularVelocity = Vector3Extensions.zero;
                    }

                    // Reset locosphere and knee pos so the rig doesn't get stuck
                    physRig.knee.transform.position = pos;
                    physRig.feet.transform.position = pos;

                    pelvisPDController.Reset();
                }
            }
            catch (Exception e)
            {
#if DEBUG
                FusionLogger.LogException("executing OnPelvisPin", e);
#endif

                // Just ignore these. Don't really matter.
            }
        }

        private void OnRepUpdate()
        {
            if (!IsCreated)
                return;

            OnHandUpdate(RigReferences.LeftHand);
            OnHandUpdate(RigReferences.RightHand);

            OnUpdateVoiceSource();
        }

        private void OnRepFixedUpdate()
        {
            if (!IsCreated)
            {
                return;
            }

            OnPelvisPin();
        }

        private void OnRepLateUpdate()
        {
            if (!IsCreated)
            {
                serializedPelvis = default;
                return;
            }

            OnUpdateNametags();

            // Update the player if its dirty and has an avatar
            var rm = RigReferences.RigManager;

            // Swap the avatar
            if (_isAvatarDirty)
            {
                RigReferences.SwapAvatarCrate(avatarId, OnSwapAvatar, OnPrepareAvatar);
                _isAvatarDirty = false;

                PlayerAdditionsHelper.OnAvatarChanged(rm);
            }

            // Change body vitals
            if (_isVitalsDirty)
            {
                vitals?.CopyTo(rm.GetComponentInChildren<BodyVitals>());

                _isVitalsDirty = false;
            }

            // Toggle ragdoll mode
            if (_isRagdollDirty)
            {
                if (_ragdollState)
                    rm.physicsRig.RagdollRig();
                else
                    rm.physicsRig.UnRagdollRig();

                _isRagdollDirty = false;
            }

            // Update settings
            if (_isSettingsDirty)
            {
                if (playerSettings != null)
                {
                    // Make sure the alpha is 1 so that people cannot create invisible names
                    var color = playerSettings.nametagColor;
                    color.a = 1f;
                    repNameText.color = color;
                }

                _isSettingsDirty = false;
            }

            // Update server side settings
            if (_isServerDirty)
            {
                repCanvas.gameObject.SetActive(FusionPreferences.NametagsEnabled && FusionOverrides.ValidateNametag(PlayerId));

                _isServerDirty = false;
            }

            // Update distance value
            DistanceSqr = (RigReferences.Head.position - RigData.RigReferences.Head.position).sqrMagnitude;
        }

        public static void OnUpdate()
        {
            for (var i = 0; i < PlayerRepManager.PlayerReps.Count; i++)
                PlayerRepManager.PlayerReps[i].OnRepUpdate();
        }

        public static void OnFixedUpdate()
        {
            for (var i = 0; i < PlayerRepManager.PlayerReps.Count; i++)
                PlayerRepManager.PlayerReps[i].OnRepFixedUpdate();
        }

        public static void OnLateUpdate()
        {
            for (var i = 0; i < PlayerRepManager.PlayerReps.Count; i++)
                PlayerRepManager.PlayerReps[i].OnRepLateUpdate();
        }

        /// <summary>
        /// Completely destroys the PlayerRep.
        /// </summary>
        public void Cleanup()
        {
            UnhookMethods();

            DestroyRep();

#if DEBUG
            FusionLogger.Log($"Cleaned up PlayerRep with small id {PlayerId.SmallId}");
#endif
        }

        /// <summary>
        /// Destroys the GameObjects of the PlayerRep. Does not destroy the PlayerRep in its entirety.
        /// </summary>
        public void DestroyRep()
        {
            if (IsCreated)
            {
                RigReferences.LeftHand.TryDetach();
                RigReferences.RightHand.TryDetach();

                GameObject.Destroy(RigReferences.RigManager.gameObject);
            }

            if (!repCanvas.IsNOC())
                GameObject.Destroy(repCanvas.gameObject);
        }
    }
}
