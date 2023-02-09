using System;
using System.Collections.Generic;

using LabFusion.Data;
using LabFusion.Debugging;
using LabFusion.Extensions;
using LabFusion.Grabbables;
using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.Utilities;

using PuppetMasta;

using SLZ;
using SLZ.AI;
using SLZ.Interaction;
using SLZ.Marrow.Pool;
using SLZ.Marrow.SceneStreaming;
using SLZ.Props;
using SLZ.Props.Weapons;
using SLZ.Utilities;
using SLZ.Vehicle;

using UnityEngine;

namespace LabFusion.Syncables
{
    public class PropSyncable : ISyncable {
        public static readonly FusionComponentCache<GameObject, PropSyncable> Cache = new FusionComponentCache<GameObject, PropSyncable>();
        public static readonly FusionComponentCache<GameObject, PropSyncable> HostCache = new FusionComponentCache<GameObject, PropSyncable>();

        public Grip[] PropGrips;
        public Rigidbody[] Rigidbodies;
        public FixedJoint[] LockJoints;
        public PDController[] PDControllers;

        public GameObject[] HostGameObjects;
        public Transform[] HostTransforms;

        private bool[] RigidbodyNulls;

        public readonly AssetPoolee AssetPoolee;

        public readonly GameObject GameObject;

        public bool DisableSyncing = false;
        public readonly bool HasIgnoreHierarchy;

        public float TimeOfMessage = 0f;

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

        private IReadOnlyList<IPropExtender> _extenders;

        private GrabbedGripList _grabbedGrips;

        private Action<ulong> _catchupDelegate;

        public PropSyncable(InteractableHost host = null, GameObject root = null) {
            if (root != null)
                GameObject = root;
            else if (host != null)
                GameObject = host.GetSyncRoot();

            AssetPoolee = AssetPoolee.Cache.Get(GameObject);

            if (Cache.TryGet(GameObject, out var syncable))
                SyncManager.RemoveSyncable(syncable);

            Cache.Add(GameObject, this);

            // Recreate all rigidbodies incase of them being gone (ascent Amber ball, looking at you)
            var tempHosts = GameObject.GetComponentsInChildren<InteractableHost>(true);
            foreach (var tempHost in tempHosts) {
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
            PDControllers = new PDController[Rigidbodies.Length];
            LockJoints = new FixedJoint[Rigidbodies.Length];

            for (var i = 0; i < Rigidbodies.Length; i++) {
                // Get the GameObject info
                HostGameObjects[i] = Rigidbodies[i].gameObject;
                HostTransforms[i] = HostGameObjects[i].transform;

                HostCache.Add(HostGameObjects[i], this);

                PDControllers[i] = new PDController();
            }

            HasIgnoreHierarchy = GameObject.GetComponentInParent<IgnoreHierarchy>(true);

            _extenders = PropExtenderManager.GetPropExtenders(this);
        }

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
            RigidbodyNulls = new bool[Rigidbodies.Length];

            _grabbedGrips = new GrabbedGripList(PropGrips.Length);
        }

        public void OnTransferOwner(Hand hand) {
            // Determine the manager
            // Main player
            if (hand.manager == RigData.RigReferences.RigManager) {
                SyncManager.SendOwnershipTransfer(GetId());
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

            _grabbedGrips.OnGripDetach(grip);

            foreach (var extender in _extenders)
                extender.OnDetach(hand, grip);
        }

        public void Cleanup() {
            if (!GameObject.IsNOC()) {
                Cache.Remove(GameObject);
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
            // Reset position info
            if (Owner == null)
                FreezeValues();
            else if (Owner != owner)
                NullValues();

            byte? prevOwner = Owner;

            Owner = owner;

            _isLockingDirty = true;
            _lockedState = false;

            TimeOfMessage = Time.realtimeSinceStartup;

            // Notify extenders about ownership transfer
            if (prevOwner != Owner) {
                foreach (var extender in _extenders)
                    extender.OnOwnershipTransfer();
            }
        }

        public void FreezeValues() {
            for (var i = 0; i < Rigidbodies.Length; i++) {
                DesiredPositions[i] = HostTransforms[i].position;
                DesiredRotations[i] = HostTransforms[i].rotation;
                DesiredVelocities[i] = Vector3.zero;
                DesiredAngularVelocities[i] = Vector3.zero;
                InitialPositions[i] = HostTransforms[i].position;
                InitialRotations[i] = HostTransforms[i].rotation;
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

        public void VerifyOwner() {
            if (Owner.HasValue && PlayerIdManager.GetPlayerId(Owner.Value) == null)
                Owner = null;
        }

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

        public bool IsQueued() {
            return SyncManager.QueuedSyncables.ContainsValue(this);
        }

        public bool IsRegistered() => _hasRegistered;

        private bool HasValidParameters() => !DisableSyncing && _hasRegistered && LevelWarehouseUtilities.IsLoadDone() && !GameObject.IsNOC() && GameObject.activeInHierarchy;

        public void OnFixedUpdate() {
            if (!Owner.HasValue || Owner.Value == PlayerIdManager.LocalSmallId || !HasValidParameters())
                return;

            OnReceivedUpdate();
        }

        public void OnUpdate()
        {
            if (!HasValidParameters())
                return;

            foreach (var extender in _extenders)
                extender.OnUpdate();

            // Update grabbing for extenders
            if (_grabbedGrips.HasGrabbedGrips) {
                foreach (var extender in _extenders)
                    extender.OnHeld();
            }

            CheckNulls();

            VerifyOwner();
            VerifyRigidbodies();
            VerifyLocking();

            if (Owner.HasValue && Owner.Value == PlayerIdManager.LocalSmallId) {
                OnOwnedUpdate();
            }
        }

        public void PushUpdate() {
            _grabbedGrips.OnPushUpdate();
        }

        public void CheckNulls() {
            for (var i = 0; i < Rigidbodies.Length; i++) {
                RigidbodyNulls[i] = Rigidbodies[i] == null;
            }
        }

        public bool IsRigidbodyNull(int index) {
            return RigidbodyNulls[index];
        }

        private bool HasMoved(int index)
        {
            var transform = HostTransforms[index];
            var lastPosition = LastSentPositions[index];
            var lastRotation = LastSentRotations[index];

            return (transform.position - lastPosition).sqrMagnitude > 0.01f || Quaternion.Angle(transform.rotation, lastRotation) > 0.15f;
        }

        public bool IsMissingRigidbodies() {
            foreach (bool value in RigidbodyNulls) {
                if (value)
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
                if (IsRigidbodyNull(i)) {
                    continue;
                }

                var rb = Rigidbodies[i];

                if (!hasMovingBody && !rb.IsSleeping() && HasMoved(i)) {
                    hasMovingBody = true;
                    break;
                }
            }

            if (!hasMovingBody)
                return;

            for (var i = 0; i < HostTransforms.Length; i++) {
                var transform = HostTransforms[i];

                LastSentPositions[i] = transform.position;
                LastSentRotations[i] = transform.rotation;
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

        private void OnReceivedUpdate() {
            if (!SafetyUtilities.IsValidTime)
                return;

            float dt = Time.fixedDeltaTime;
            float timeSinceMessage = Time.realtimeSinceStartup - TimeOfMessage;

            foreach (var extender in _extenders)
                extender.OnReceivedUpdate();

            // Check if anything is being grabbed
            if (!_grabbedGrips.HasGrabbedGrips && timeSinceMessage >= 1f) {
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
                if (IsRigidbodyNull(i))
                    continue;

                var rb = Rigidbodies[i];
                var transform = HostTransforms[i];

                bool isGrabbed = false;

                if (_grabbedGrips.HasGrabbedGrips) {
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
                // If so, just set positions
                if (rb.isKinematic) {
                    if (allowPosition)
                        rb.MovePosition(pos);

                    rb.MoveRotation(rot);
                    continue;
                }

                var pdController = PDControllers[i];
                
                // Don't over predict
                if (timeSinceMessage <= 0.6f) {
                    // Move position with prediction
                    if (allowPosition)
                    {
                        pos += vel * dt;
                        DesiredPositions[i] = pos;
                    }

                    // Move rotation with prediction
                    rot = (angVel * dt).GetQuaternionDisplacement() * rot;
                    DesiredRotations[i] = rot;
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
                float distSqr = (transform.position - pos).sqrMagnitude;
                if (distSqr > (2f * (vel.sqrMagnitude + 1f)) && allowPosition) {
                    transform.position = pos;
                    transform.rotation = rot;

                    rb.velocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }
                // Instead calculate velocity stuff
                else {
                    if (allowPosition) {
                        rb.AddForce(pdController.GetForce(rb, transform, pos, vel), ForceMode.Acceleration);
                    }
                    else {
                        if (rb.useGravity)
                            rb.AddForce(-Physics.gravity, ForceMode.Acceleration);
                    }

                    rb.AddTorque(pdController.GetTorque(rb, transform, rot, angVel), ForceMode.Acceleration);
                }
            }
        }
    }
}
