using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using LabFusion.Data;
using LabFusion.Debugging;
using LabFusion.Extensions;
using LabFusion.Grabbables;
using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.Senders;
using LabFusion.Utilities;

using SLZ.Interaction;
using SLZ.Marrow.Pool;
using SLZ.Utilities;

using UnityEngine;

namespace LabFusion.Syncables
{
    public class PropSyncable : ISyncable {
        private enum SendState {
            IDLE = 0,
            SENDING = 1,
        }

        public const float MinMoveMagnitude = 0.005f;
        public const float MinMoveSqrMagnitude = MinMoveMagnitude * MinMoveMagnitude;
        public const float MinMoveAngle = 0.15f;

        public static readonly FusionComponentCache<GameObject, PropSyncable> Cache = new FusionComponentCache<GameObject, PropSyncable>();
        public static readonly FusionComponentCache<GameObject, PropSyncable> HostCache = new FusionComponentCache<GameObject, PropSyncable>();

        public bool IsVehicle = false;

        public Grip[] PropGrips;
        public Rigidbody[] Rigidbodies;
        public FixedJoint[] LockJoints;
        public PDController[] PDControllers;

        public GameObject[] HostGameObjects;
        public Transform[] HostTransforms;
        public TransformCache[] TransformCaches;
        public RigidbodyCache[] RigidbodyCaches;

        public readonly AssetPoolee AssetPoolee;

        public readonly GameObject GameObject;

        public bool IsRootEnabled;

        public PropLifeCycleEvents LifeCycleEvents;

        public bool DisableSyncing = false;
        public readonly bool HasIgnoreHierarchy;

        public float TimeOfMessage = 0f;

        public bool IsSleeping = false;

        public ushort Id;

        public byte? Owner = null;

        // Target info
        public Vector3?[] InitialPositions;
        public Quaternion?[] InitialRotations;

        public Vector3?[] DesiredPositions;
        public Quaternion?[] DesiredRotations;
        public Vector3?[] DesiredVelocities;
        public Vector3?[] DesiredAngularVelocities;

        // Last sent info
        public Vector3[] LastSentPositions;
        public Quaternion[] LastSentRotations;

        private bool _verifyRigidbodies;

        private bool _hasRegistered = false;

        private bool _isLockingDirty = false;
        private bool _lockedState = false;

        private bool _isIgnoringForces = false;

        private SendState _sendingState = SendState.IDLE;
        private float _timeOfLastSend = 0f;

        private IReadOnlyList<IPropExtender> _extenders;

        private GrabbedGripList _grabbedGrips;

        public bool IsHeld = false;

        private Action<ulong> _catchupDelegate;

        private bool _wasDisposed = false;

        private const int _targetFrame = 3;
        private readonly FrameSkipper _predictionSkipper = new FrameSkipper(_targetFrame);

        public PropSyncable(InteractableHost host = null, GameObject root = null) {
            if (root != null)
                GameObject = root;
            else if (host != null)
                GameObject = host.GetSyncRoot();

            AssetPoolee = AssetPoolee.Cache.Get(GameObject);

            if (Cache.TryGet(GameObject, out var syncable))
                SyncManager.RemoveSyncable(syncable);

            Cache.Add(GameObject, this);

            LifeCycleEvents = GameObject.AddComponent<PropLifeCycleEvents>();
            LifeCycleEvents.enabled = false;
            LifeCycleEvents.Syncable = this;
            LifeCycleEvents.enabled = true;

            // Recreate all rigidbodies incase of them being gone (ascent Amber ball, looking at you)
            var tempHosts = GameObject.GetComponentsInChildren<InteractableHost>(true);
            foreach (var tempHost in tempHosts) {
                if (tempHost.IsStatic)
                    continue;

                tempHost.CreateRigidbody();
                tempHost.EnableInteraction();

                // Remove from key lists
                if (KeyReciever.ClaimedHosts != null) {
                    KeyReciever.ClaimedHosts.Remove(tempHost.TryCast<IGrippable>());
                }
            }

            // Assign grip, rigidbody, etc. info
            if (GameObject) 
                AssignInformation(GameObject);
            
            foreach (var grip in PropGrips) {
                grip.attachedHandDelegate += (Grip.HandDelegate)((h) => { OnAttach(h, grip); });
                grip.detachedHandDelegate += (Grip.HandDelegate)((h) => { OnDetach(h, grip); });
            }

            // Setup target arrays
            InitialPositions = new Vector3?[Rigidbodies.Length];
            InitialRotations = new Quaternion?[Rigidbodies.Length];
            DesiredPositions = new Vector3?[Rigidbodies.Length];
            DesiredRotations = new Quaternion?[Rigidbodies.Length];
            DesiredVelocities = new Vector3?[Rigidbodies.Length];
            DesiredAngularVelocities = new Vector3?[Rigidbodies.Length];

            LastSentPositions = new Vector3[Rigidbodies.Length];
            LastSentRotations = new Quaternion[Rigidbodies.Length];

            // Setup gameobject arrays
            HostGameObjects = new GameObject[Rigidbodies.Length];
            HostTransforms = new Transform[Rigidbodies.Length];
            TransformCaches = new TransformCache[Rigidbodies.Length];
            RigidbodyCaches = new RigidbodyCache[Rigidbodies.Length];
            PDControllers = new PDController[Rigidbodies.Length];
            LockJoints = new FixedJoint[Rigidbodies.Length];

            for (var i = 0; i < Rigidbodies.Length; i++) {
                // Clear out potential conflicting syncables
                var go = Rigidbodies[i].gameObject;
                if (HostCache.TryGet(go, out var conflict) && conflict != this)
                    SyncManager.RemoveSyncable(conflict);

                // Get the GameObject info
                HostGameObjects[i] = go;
                HostTransforms[i] = go.transform;

                HostCache.Add(go, this);

                PDControllers[i] = new PDController();

                // Initialize transform caches
                TransformCaches[i] = new TransformCache();
                RigidbodyCaches[i] = new RigidbodyCache();

                TransformCaches[i].FixedUpdate(HostTransforms[i]);
                RigidbodyCaches[i].FixedUpdate(Rigidbodies[i]);
            }

            HasIgnoreHierarchy = GameObject.GetComponentInParent<IgnoreHierarchy>(true);

            _extenders = PropExtenderManager.GetPropExtenders(this);
        }

        public bool IsDestroyed() => _wasDisposed;

        public void InsertCatchupDelegate(Action<ulong> catchup) {
            _catchupDelegate += catchup;
        }

        public void InvokeCatchup(ulong user) {
            // Make sure this object wasn't destroyed or despawned before catching up
            if (!GameObject.IsNOC() && GameObject.activeInHierarchy)
                _catchupDelegate?.InvokeSafe(user, "executing Catchup Delegate");
        }

        public bool TryGetExtender<T>(out T extender) where T : IPropExtender {
            foreach (var found in _extenders) {
                if (found.GetType() == typeof(T)) {
                    extender = (T)found;
                    return true;
                }
            }

            extender = default;
            return false;
        }

        private void AssignInformation(GameObject go) {
            PropGrips = go.GetComponentsInChildren<Grip>(true);
            Rigidbodies = go.GetComponentsInChildren<Rigidbody>(true);

            _grabbedGrips = new GrabbedGripList(this, PropGrips.Length);
        }

        public void OnTransferOwner(Hand hand) {
            // Determine the manager
            // Main player
            if (hand.manager == RigData.RigReferences.RigManager) {
                PropSender.SendOwnershipTransfer(this);
            }

            _verifyRigidbodies = true;
        }

        public void OnAttach(Hand hand, Grip grip) {
            OnTransferOwner(hand);

            _grabbedGrips.OnGripAttach(hand, grip);

            foreach (var extender in _extenders)
                extender.OnAttach(hand, grip);
        }

        public void OnDetach(Hand hand, Grip grip) {
            OnTransferOwner(hand);

            _grabbedGrips.OnGripDetach(hand, grip);

            foreach (var extender in _extenders)
                extender.OnDetach(hand, grip);
        }

        public void Cleanup() {
            if (IsDestroyed()) {
#if DEBUG
                FusionLogger.Warn("Tried destroying a PropSyncable, but it was already destroyed!");
#endif
                return;
            }

            if (!GameObject.IsNOC()) {
                Cache.Remove(GameObject);
            }

            if (!LifeCycleEvents.IsNOC()) {
                GameObject.Destroy(LifeCycleEvents);
            }

            foreach (var host in HostGameObjects) {
                if (host == null)
                    continue;

                HostCache.Remove(host);
            }

            foreach (var extender in _extenders) {
                extender.OnCleanup();
            }

            foreach (var joint in LockJoints) {
                if (joint != null)
                    GameObject.Destroy(joint);
            }

            _wasDisposed = true;
        }

        public Grip GetGrip(ushort index) {
            if (PropGrips != null && PropGrips.Length > index)
                return PropGrips[index];
            return null;
        }

        public bool IsGrabbed() {
            foreach (var grip in PropGrips) {
                if (grip.attachedHands.Count > 0)
                    return true;
            }

            return false;
        }

        public byte? GetOwner() => Owner;

        public void SetOwner(byte owner) {
            // Make sure this isn't destroyed
            if (IsDestroyed()) {
#if DEBUG
                FusionLogger.Warn("Tried setting the owner of a PropSyncable, but it was destroyed!");
#endif
                return;
            }

            // Reset position info
            if (Owner == null)
                FreezeValues();

            byte? prevOwner = Owner;

            Owner = owner;

            _isLockingDirty = true;
            _lockedState = false;

            RefreshMessageTime();

            // Notify extenders about ownership transfer
            if (prevOwner != Owner) {
                foreach (var extender in _extenders)
                    extender.OnOwnershipTransfer();
            }
        }

        public void RemoveOwner() {
            Owner = null;
        }

        public void FreezeValues() {
            for (var i = 0; i < Rigidbodies.Length; i++) {
                var transform = TransformCaches[i];

                DesiredPositions[i] = transform.Position;
                DesiredRotations[i] = transform.Rotation;
                DesiredVelocities[i] = Vector3Extensions.zero;
                DesiredAngularVelocities[i] = Vector3Extensions.zero;
                InitialPositions[i] = transform.Position;
                InitialRotations[i] = transform.Rotation;
            }
        }

        public void NullValues() {
            for (var i = 0; i < Rigidbodies.Length; i++) {
                DesiredPositions[i] = null;
                DesiredRotations[i] = null;
                DesiredVelocities[i] = null;
                DesiredAngularVelocities[i] = null;
                InitialPositions[i] = null;
                InitialRotations[i] = null;
            }
        }

        public bool IsOwner() => Owner.HasValue && Owner.Value == PlayerIdManager.LocalSmallId;

        public void SetRigidbodiesDirty() {
            _verifyRigidbodies = true;
        }

        public void VerifyRigidbodies() {
            if (_verifyRigidbodies) {
                // Check if any are missing
                bool needToUpdate = false;
                foreach (var rb in Rigidbodies) {
                    if (rb.IsNOC()) {
                        needToUpdate = true;
                        break;
                    }
                }

                // Re-get all rigidbodies
                if (needToUpdate) {
                    for (var i = 0; i < HostGameObjects.Length; i++) {
                        var host = HostGameObjects[i];

                        if (!host.IsNOC())
                            Rigidbodies[i] = host.GetComponent<Rigidbody>();

                        RigidbodyCaches[i].VerifyNull(Rigidbodies[i]);
                    }
                }

                _verifyRigidbodies = false;
            }
        }

        private void VerifyLocking() {
            if (_isLockingDirty) {
                for (var i = 0; i < Rigidbodies.Length; i++) {
                    if (LockJoints[i] != null)
                        GameObject.Destroy(LockJoints[i]);

                    var rb = Rigidbodies[i];
                    var gameObject = HostGameObjects[i];
                    var transform = HostTransforms[i];

                    var lockPos = InitialPositions[i];
                    var lockRot = InitialRotations[i];

                    if (rb && _lockedState && lockPos.HasValue && lockRot.HasValue) {

                        transform.SetPositionAndRotation(lockPos.Value, lockRot.Value);

                        LockJoints[i] = gameObject.AddComponent<FixedJoint>();
                    }
                }

                _isLockingDirty = false;
            }
        }

        public void OnRegister(ushort id) {
            Id = id;
            _hasRegistered = true;
        }

        public ushort GetId() {
            return Id;
        }

        public ushort? GetIndex(Grip grip)
        {
            for (ushort i = 0; i < PropGrips.Length; i++)
            {
                if (PropGrips[i] == grip)
                    return i;
            }
            return null;
        }

        public ushort? GetIndex(GameObject go) {
            for (ushort i = 0; i < HostGameObjects.Length; i++)
            {
                if (HostGameObjects[i] == go)
                    return i;
            }
            return null;
        }

        public GameObject GetHost(ushort index)
        {
            if (HostGameObjects != null && HostGameObjects.Length > index)
                return HostGameObjects[index];
            return null;
        }

        public bool IsQueued() => SyncManager.QueuedSyncables.ContainsValue(this) && !IsDestroyed();
        public bool IsRegistered() => _hasRegistered;

        private bool HasValidParameters() => !DisableSyncing && _hasRegistered && FusionSceneManager.IsLoadDone() && IsRootEnabled;

        public void OnFixedUpdate() {
            try {
                if (!HasValidParameters())
                    return;

                OnFixedUpdateCache();

                if (!Owner.HasValue || Owner.Value == PlayerIdManager.LocalSmallId)
                    return;

                OnReceivedUpdate();
            }
            catch (Exception e) {
                if (e is NullReferenceException)
                    OnExceptionThrown();
                else {
                    throw e;
                }
            }
        }

        private void OnExceptionThrown() {
            for (var i = 0; i < RigidbodyCaches.Length; i++) {
                RigidbodyCaches[i].VerifyNull(Rigidbodies[i]);
            }
        }

        private void OnFixedUpdateCache() {
            for (var i = 0; i < TransformCaches.Length; i++) {
                TransformCaches[i].FixedUpdate(HostTransforms[i]);
                RigidbodyCaches[i].FixedUpdate(Rigidbodies[i]);
            }
        }

        public void OnUpdate()
        {
            try {
                if (!HasValidParameters())
                    return;

                foreach (var extender in _extenders)
                    extender.OnUpdate();

                // Update grabbing for extenders
                if (IsHeld) {
                    foreach (var extender in _extenders)
                        extender.OnHeld();
                }

                VerifyRigidbodies();
                VerifyLocking();

                if (IsOwner()) {
                    OnOwnedUpdate();
                }
            }
            catch (Exception e) {
                if (e is NullReferenceException)
                    OnExceptionThrown();
                else {
                    throw e;
                }
            }
        }

        public void PushUpdate() {
            _grabbedGrips.OnPushUpdate();
        }

        private bool HasMoved(int index)
        {
            var cache = TransformCaches[index];
            var lastPosition = LastSentPositions[index];
            var lastRotation = LastSentRotations[index];

            return (cache.Position - lastPosition).sqrMagnitude > MinMoveSqrMagnitude || Quaternion.Angle(cache.Rotation, lastRotation) > MinMoveAngle;
        }

        public bool IsMissingRigidbodies() {
            foreach (var cache in RigidbodyCaches) {
                if (cache.IsNull)
                    return true;
            }
            
            return false;
        }

        private void OnOwnedUpdate() {
            foreach (var extender in _extenders)
                extender.OnOwnedUpdate();

            NullValues();

            bool hasMovingBody = false;

            for (var i = 0; i < Rigidbodies.Length; i++) {
                var cache = RigidbodyCaches[i];

                if (cache.IsNull) {
                    continue;
                }

                var rb = Rigidbodies[i];

                // Don't sync kinematic rigidbodies
                if (rb.isKinematic)
                    continue;

                if (!hasMovingBody && !cache.IsSleeping && HasMoved(i)) {
                    hasMovingBody = true;
                    break;
                }
            }

            // Update send time
            if (hasMovingBody)
                _timeOfLastSend = Time.realtimeSinceStartup;

            // If a rigidbody has not moved within half a second, stop sending
            if (Time.realtimeSinceStartup - _timeOfLastSend >= 0.5f) {
                SetSendState(SendState.IDLE);
                return;
            }
            else {
                SetSendState(SendState.SENDING);
            }

            for (var i = 0; i < TransformCaches.Length; i++) {
                var cache = TransformCaches[i];

                LastSentPositions[i] = cache.Position;
                LastSentRotations[i] = cache.Rotation;
            }

            using (var writer = FusionWriter.Create(PropSyncableUpdateData.DefaultSize + (PropSyncableUpdateData.RigidbodySize * Rigidbodies.Length))) {
                using (var data = PropSyncableUpdateData.Create(PlayerIdManager.LocalSmallId, this)) {
                    writer.Write(data);

                    using (var message = FusionMessage.Create(NativeMessageTag.PropSyncableUpdate, writer)) {
                        MessageSender.BroadcastMessageExceptSelf(NetworkChannel.Unreliable, message);
                    }
                }
            }
        }

        private void SetSendState(SendState state) {
            if (_sendingState == state)
                return;

            switch (state) {
                default:
                case SendState.IDLE:
                    PropSender.SendSleep(this);
                    break;
                case SendState.SENDING:
                    break;
            }

            _sendingState = state;
        }

        public void RefreshMessageTime() {
            TimeOfMessage = Time.realtimeSinceStartup;
            IsSleeping = false;
        }

        private void OnReceivedUpdate() {
            if (!SafetyUtilities.IsValidTime)
                return;

            float dt = Time.fixedDeltaTime;
            float timeSinceMessage = Time.realtimeSinceStartup - TimeOfMessage;

            foreach (var extender in _extenders)
                extender.OnReceivedUpdate();

            // Check if anything is being grabbed
            if (!IsHeld && (IsSleeping || timeSinceMessage >= 1f)) {
                if (!_isIgnoringForces) {
                    // Set all desired values to nothing
                    for (var i = 0; i < Rigidbodies.Length; i++) {
                        DesiredPositions[i] = null;
                        DesiredRotations[i] = null;
                        DesiredVelocities[i] = null;
                        DesiredAngularVelocities[i] = null;
                    }

                    _isLockingDirty = true;
                    _lockedState = true;
                    _isIgnoringForces = true;
                }
                return;
            }

            if (_isIgnoringForces) {
                _isLockingDirty = true;
                _lockedState = false;
                _isIgnoringForces = false;
            }

            for (var i = 0; i < Rigidbodies.Length; i++) {
                var rbCache = RigidbodyCaches[i];

                if (rbCache.IsNull)
                    continue;

                var rb = Rigidbodies[i];
                var transform = HostTransforms[i];
                var cache = TransformCaches[i];

                bool isGrabbed = false;

                if (IsHeld) {
                    foreach (var grip in _grabbedGrips.GetGrabbedGrips()) {
                        if (grip.Host.Rb == rb) {
                            DesiredPositions[i] = null;
                            DesiredRotations[i] = null;
                            isGrabbed = true;
                            break;
                        }
                    }
                }

                if (isGrabbed || !DesiredPositions[i].HasValue || !DesiredRotations[i].HasValue) {
                    continue;
                }

                var pos = DesiredPositions[i].Value;
                var rot = DesiredRotations[i].Value;
                var vel = DesiredVelocities[i].Value;
                var angVel = DesiredAngularVelocities[i].Value;

                bool allowPosition = !HasIgnoreHierarchy;

                // Check if this is kinematic
                // If so, just ignore values
                if (rb.isKinematic) {
                    continue;
                }

                var pdController = PDControllers[i];
                
                // Don't over predict
                if (timeSinceMessage <= 0.6f) {
                    // Move position with prediction
                    if (allowPosition) {
                        pos += vel * dt;
                        DesiredPositions[i] = pos;
                    }

                    // Only predict rotation every so often
                    if (_predictionSkipper.IsMatchingFrame()) {
                        // Move rotation with prediction
                        rot = (angVel * dt * _targetFrame).GetQuaternionDisplacement() * rot;
                        DesiredRotations[i] = rot;
                    }
                }
                else {
                    // Reset transform values
                    if (allowPosition) {
                        pos = InitialPositions[i].Value;
                        DesiredPositions[i] = pos;
                    }

                    rot = InitialRotations[i].Value;
                    DesiredRotations[i] = rot;
                }

                // Teleport check
                float distSqr = (cache.Position - pos).sqrMagnitude;
                if (distSqr > (2f * (vel.sqrMagnitude + 1f)) && allowPosition) {
                    transform.position = pos;
                    transform.rotation = rot;

                    rb.velocity = Vector3Extensions.zero;
                    rb.angularVelocity = Vector3Extensions.zero;
                }
                // Instead calculate velocity stuff
                else {
                    if (allowPosition) {
                        rb.AddForce(pdController.GetForce(rb, cache.Position, rbCache.Velocity, pos, vel), ForceMode.Acceleration);
                    }
                    else {
                        if (rb.useGravity)
                            rb.AddForce(-PhysicsUtilities.Gravity, ForceMode.Acceleration);
                    }

                    rb.AddTorque(pdController.GetTorque(rb, cache.Rotation, rbCache.AngularVelocity, rot, angVel), ForceMode.Acceleration);
                }
            }
        }
    }
}
