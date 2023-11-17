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

using SLZ.Marrow.Warehouse;
using SLZ.VRMK;

using Avatar = SLZ.VRMK.Avatar;

using SystemVector3 = System.Numerics.Vector3;
using SystemQuaternion = System.Numerics.Quaternion;

namespace LabFusion.Representation
{
    public class PlayerRep : IDisposable
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

        public static Transform[] syncedPoints = null;
        public static Transform[] gameworldPoints = null;
        public static Transform syncedPlayspace;
        public static Transform syncedPelvis;
        public static Hand syncedLeftHand;
        public static Hand syncedRightHand;

        public SerializedLocalTransform[] serializedLocalTransforms = new SerializedLocalTransform[RigAbstractor.TransformSyncCount];
        public SerializedLocalTransform[] serializedGameworldLocalTransforms = new SerializedLocalTransform[RigAbstractor.GameworldRigTransformCount];
        public SerializedTransform serializedPelvis;

        public SystemVector3 predictVelocity;
        public SystemVector3 predictAngularVelocity;

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

        private bool _isQuestUser = false;

        // Voice chat integration
        private const float _voiceUpdateStep = 0.3f;

        private float _maxMicrophoneDistance = 30f;

        private AudioSource _voiceSource = null;
        private bool _hasVoice = false;

        private bool _spatialized = false;

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

            _isQuestUser = PlayerId.GetMetadata(MetadataHelper.PlatformKey) == "QUEST";

            pelvisPDController = new PDController();

            MultiplayerHooking.OnServerSettingsChanged += OnServerSettingsChanged;
            FusionOverrides.OnOverridesChanged += OnOverridesChanged;

            ResetSerializedTransforms();

            StartRepCreation();
        }

        public void InsertVoiceSource(AudioSource source)
        {
            _voiceSource = source;
            _hasVoice = true;
        }

        public float GetVoiceLoudness() => _voiceLoudness;

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
                _voiceSource.reverbZoneMix = 0.35f;
                _voiceSource.dopplerLevel = 0.5f;

                _spatialized = false;
                MicrophoneDisabled = false;
            }

            // Update the jaw movement
            if (MicrophoneDisabled)
            {
                _voiceLoudness = 0f;
            }
            else
            {
                OnUpdateVoiceLoudness();
            }
        }

        private void OnUpdateVoiceLoudness()
        {
            // Update the amplitude
            _voiceUpdateTime += TimeUtilities.DeltaTime;
            if (_voiceUpdateTime >= _voiceUpdateStep)
            {
                _voiceUpdateTime = 0f;

                var spectrum = _voiceSource.GetSpectrumData(256, 0, FFTWindow.Rectangular);
                int length = spectrum.Length;

                float gain = 0f;
                for (var i = 0; i < length; i++)
                {
                    gain += Math.Abs(spectrum[i]);
                }

                if (length > 0)
                    gain /= (float)length;

                _targetLoudness = gain;

                // Add affectors
                _targetLoudness *= 100f;
                _targetLoudness = ManagedMathf.Clamp(_targetLoudness, 0f, 2f);
            }

            // Lerp towards the desired value
            float sin = Math.Abs(_sinAmplitude * ManagedMathf.Sin(_sinOmega * TimeUtilities.TimeSinceStartup));
            sin = ManagedMathf.Clamp01(sin);

            _voiceLoudness = ManagedMathf.LerpUnclamped(_voiceLoudness * sin, _targetLoudness, TimeUtilities.DeltaTime * 12f);
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
            pullCord.PlayClip(pullCord.switchAvatar, pullCord.ap3, pullCord.switchVolume, 4f, false);
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
                _voiceSource.reverbZoneMix = ManagedMathf.Clamp(0.35f * heightMult, 0f, 1.02f);

                _maxMicrophoneDistance = 30f * heightMult;

                _voiceSource.SetRealisticRolloff(0.5f, _maxMicrophoneDistance);

                // Set the mixer
                if (_voiceSource.outputAudioMixerGroup == null)
                    _voiceSource.outputAudioMixerGroup = pullCord.mixerGroup;
            }
        }

        public void OnRigCreated(RigManager rig)
        {
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
            RigAbstractor.FillGameworldArray(ref gameworldRigTransforms, rig);

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

        public void OnHeptaBody2Update()
        {
            try
            {
                if (!IsCreated)
                    return;

                for (var i = 0; i < RigAbstractor.GameworldRigTransformCount; i++)
                {
                    var localTransform = serializedGameworldLocalTransforms[i];

                    if (!localTransform.IsValid)
                        break;

                    var pos = localTransform.position;
                    var rot = localTransform.rotation;

                    var gameworldTransform = gameworldRigTransforms[i];

                    gameworldTransform.localPosition = pos.ToUnityVector3();
                    gameworldTransform.localRotation = rot.ToUnityQuaternion();
                }
            }
            catch
            {
            }
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
            try
            {
                if (!IsCreated)
                    return;

                for (var i = 0; i < RigAbstractor.TransformSyncCount; i++)
                {
                    repTransforms[i].localPosition = serializedLocalTransforms[i].position.ToUnityVector3();
                    repTransforms[i].localRotation = serializedLocalTransforms[i].rotation.ToUnityQuaternion();
                }
            }
            catch
            {
                // Literally no reason this should happen but it does
                // Doesn't cause anything soooo

                // POST NOTE: actually yea it would happen if an item in serializedLocalTransforms is null
                // cleanup that functionality later so you can remove this try catch
            }
        }

        public void OnPelvisPin()
        {
            try
            {
                // Stop pelvis
                if (!IsCreated || !serializedPelvis.IsValid)
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
                SystemVector3 pelvisPosition = pelvisTransform.position.ToSystemVector3();
                SystemQuaternion pelvisRotation = pelvisTransform.rotation.ToSystemQuaternion();

                // Move position with prediction
                if (TimeUtilities.TimeSinceStartup - timeSincePelvisSent <= 1.5f)
                {
                    serializedPelvis.position += predictVelocity * TimeUtilities.FixedDeltaTime;

                    _hasLockedPosition = false;
                }
                else if (!_hasLockedPosition)
                {
                    serializedPelvis.position = pelvisPosition;
                    predictVelocity = SystemVector3.Zero;
                    predictAngularVelocity = SystemVector3.Zero;

                    _hasLockedPosition = true;
                }

                // Apply velocity
                if (SafetyUtilities.IsValidTime)
                {
                    var pos = serializedPelvis.position;
                    var rot = serializedPelvis.rotation;

                    repPelvis.AddForce(pelvisPDController.GetForce(repPelvis, pelvisPosition, repPelvis.velocity.ToSystemVector3(), pos, predictVelocity).ToUnityVector3(), ForceMode.Acceleration);
                    // We only want to apply angular force when ragdolled
                    if (rigManager.physicsRig.torso.spineInternalMult <= 0f)
                    {
                        repPelvis.AddTorque(pelvisPDController.GetTorque(repPelvis, pelvisRotation, repPelvis.angularVelocity.ToSystemVector3(), rot, predictAngularVelocity).ToUnityVector3(), ForceMode.Acceleration);
                    }
                    else
                        pelvisPDController.ResetRotation();
                }

                // Check for stability teleport
                float distSqr = (pelvisPosition - serializedPelvis.position).LengthSquared();
                if (distSqr > (2f * (predictVelocity.Length() + 1f)))
                {
                    // Get teleport position
                    var pos = serializedPelvis.position.ToUnityVector3();
                    var physRig = RigReferences.RigManager.physicsRig;

                    // Offset
                    pos += physRig.feet.transform.position - physRig.m_pelvis.position;
                    pos += physRig.footballRadius * -physRig.m_pelvis.up;

                    RigReferences.RigManager.Teleport(pos);

                    // Zero our teleport velocity, cause the rig doesn't seem to do that on its own?
                    foreach (var rb in RigReferences.RigManager.physicsRig.GetComponentsInChildren<Rigidbody>())
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
            catch
            {
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
                    using var data = PlayerRepGameworldData.Create(PlayerIdManager.LocalSmallId, gameworldPoints);
                    writer.Write(data);

                    using var message = FusionMessage.Create(NativeMessageTag.PlayerRepGameworld, writer);
                    MessageSender.BroadcastMessageExceptSelf(NetworkChannel.Unreliable, message);
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

        private static bool TrySendRep()
        {
            try
            {
                if (syncedPoints == null || PlayerIdManager.LocalId == null)
                    return false;

                using (var writer = FusionWriter.Create(PlayerRepTransformData.Size))
                {
                    using var data = PlayerRepTransformData.Create(PlayerIdManager.LocalSmallId, syncedPoints, syncedPelvis, syncedPlayspace, syncedLeftHand, syncedRightHand);
                    writer.Write(data);

                    using var message = FusionMessage.Create(NativeMessageTag.PlayerRepTransform, writer);
                    MessageSender.BroadcastMessageExceptSelf(NetworkChannel.Unreliable, message);
                }

                return true;
            }
            catch (Exception e)
            {
#if DEBUG
                FusionLogger.Error($"Failed sending player transforms with reason: {e.Message}\nTrace:{e.StackTrace}");
#endif
            }
            return false;
        }

        public static void OnSyncRep()
        {
            if (NetworkInfo.HasServer && RigData.HasPlayer)
            {
                if (!TrySendRep())
                    OnCachePlayerTransforms();
                else if (RigData.RigReferences.RigManager.activeSeat)
                {
                    TrySendGameworldRep();
                }
            }
            else
            {
                syncedPoints = null;
                gameworldPoints = null;
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
                if (vitals != null)
                {
                    vitals.CopyTo(rm.bodyVitals);
                }

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
        /// Destroys anything about the PlayerRep and frees it from memory.
        /// </summary>
        public void Dispose()
        {
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

        public static void OnCachePlayerTransforms()
        {
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
