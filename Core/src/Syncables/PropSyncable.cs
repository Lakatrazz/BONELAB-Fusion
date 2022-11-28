using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Il2CppSystem.Diagnostics;
using LabFusion.Data;
using LabFusion.Extensions;
using LabFusion.Grabbables;
using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.Utilities;
using PuppetMasta;
using SLZ;
using SLZ.Interaction;
using SLZ.Marrow.Pool;
using SLZ.Marrow.SceneStreaming;
using SLZ.Props.Weapons;
using SLZ.Utilities;

using UnityEngine;

namespace LabFusion.Syncables
{
    public class PropSyncable : ISyncable {
        public static readonly Dictionary<GameObject, PropSyncable> Cache = new Dictionary<GameObject, PropSyncable>(new UnityComparer());
        public static readonly Dictionary<ConfigurableJoint, PropSyncable> JointCache = new Dictionary<ConfigurableJoint, PropSyncable>(new UnityComparer());
        public static readonly Dictionary<WeaponSlot, PropSyncable> WeaponSlotCache = new Dictionary<WeaponSlot, PropSyncable>(new UnityComparer());

        public const float PropPinMlp = 0.8f;

        public Grip[] PropGrips;
        public Rigidbody[] Rigidbodies;
        public GameObject[] HostGameObjects;

        public Dictionary<ConfigurableJoint, ConfigurableJointDriveData> ConfigurableJoints;

        public readonly AssetPoolee AssetPoolee;
        public readonly WeaponSlot WeaponSlot;

        public readonly GameObject GameObject;

        public readonly bool IsRotationBased;

        public float TimeOfMessage = 0f;

        public bool CanSetJointDrives => !HasValidParameters() || !Owner.HasValue || Owner.Value == PlayerIdManager.LocalSmallId;

        public ushort Id;

        public byte? Owner = null;

        // Target info
        public Vector3?[] DesiredPositions;
        public Quaternion?[] DesiredRotations;
        public float DesiredVelocity;

        // Last sent info
        public Vector3[] LastSentPositions;
        public Quaternion[] LastSentRotations;

        private bool _verifyRigidbodies;

        private bool _hasRegistered = false;

        private readonly Dictionary<Grip, int> _grabbedGrips = new Dictionary<Grip, int>();

        public PropSyncable(InteractableHost host = null, GameObject root = null) {
            if (root != null)
                GameObject = root;
            else if (host != null)
                GameObject = host.GetRoot();

            AssetPoolee = AssetPoolee.Cache.Get(GameObject);

            if (Cache.ContainsKey(GameObject))
                SyncManager.RemoveSyncable(Cache[GameObject]);

            Cache.Add(GameObject, this);

            if (host) {
                if (host.manager)
                    AssignInformation(host.manager);
                else
                    AssignInformation(host);
            }
            else if (GameObject) {
                AssignInformation(GameObject);
            }

            foreach (var grip in PropGrips) {
                grip.attachedHandDelegate += (Grip.HandDelegate)((h) => { OnAttach(h, grip);});
                grip.detachedHandDelegate += (Grip.HandDelegate)((h) => { OnDetach(h, grip); });
            }

            HostGameObjects = new GameObject[Rigidbodies.Length];
            ConfigurableJoints = new Dictionary<ConfigurableJoint, ConfigurableJointDriveData>(new UnityComparer());

            for (var i = 0; i < Rigidbodies.Length; i++) {
                HostGameObjects[i] = Rigidbodies[i].gameObject;

                var joint = Rigidbodies[i].gameObject.GetComponent<ConfigurableJoint>();
                if (joint) {
                    ConfigurableJoints.Add(joint, new ConfigurableJointDriveData(joint));
                    JointCache.Add(joint, this);
                }
            }

            DesiredPositions = new Vector3?[Rigidbodies.Length];
            DesiredRotations = new Quaternion?[Rigidbodies.Length];
            DesiredVelocity = 0f;

            LastSentPositions = new Vector3[Rigidbodies.Length];
            LastSentRotations = new Quaternion[Rigidbodies.Length];

            if (GameObject.GetComponentInChildren<BehaviourPowerLegs>(true) || GameObject.GetComponentInChildren<BehaviourCrablet>(true)) {
                IsRotationBased = true;
            }
            else {
                IsRotationBased = false;
            }

            WeaponSlot = GameObject.GetComponentInChildren<WeaponSlot>(true);

            if (WeaponSlot)
                WeaponSlotCache.Add(WeaponSlot, this);
        }

        public void OnSetDrive(ConfigurableJoint joint, JointDrive drive, JointExtensions.JointDriveAxis axis) {
            if (joint.IsNOC())
                return;

            if (ConfigurableJoints.ContainsKey(joint)) {
                var data = ConfigurableJoints[joint];

                switch (axis) {
                    default:
                    case JointExtensions.JointDriveAxis.XDrive:
                        data.xDrive = drive;
                        break;
                    case JointExtensions.JointDriveAxis.YDrive:
                        data.yDrive = drive;
                        break;
                    case JointExtensions.JointDriveAxis.ZDrive:
                        data.zDrive = drive;
                        break;
                    case JointExtensions.JointDriveAxis.AngularXDrive:
                        data.angularXDrive = drive;
                        break;
                    case JointExtensions.JointDriveAxis.AngularYZDrive:
                        data.angularYZDrive = drive;
                        break;
                    case JointExtensions.JointDriveAxis.SlerpDrive:
                        data.slerpDrive = drive;
                        break;
                }

                ConfigurableJoints[joint] = data;
            }
        }

        private void AssignInformation(InteractableHost host) {
            var hosts = host.GetRoot().GetComponentsInChildren<InteractableHost>(true);

            List<Grip> grips = new List<Grip>();
            List<Rigidbody> rigidbodies = new List<Rigidbody>();

            foreach (var newHost in hosts) {
                if (newHost.Rb == null)
                    continue;

                grips.AddRange(newHost._grips.ToArray());
                rigidbodies.Add(newHost.Rb);
            }

            PropGrips = grips.ToArray();
            Rigidbodies = rigidbodies.ToArray();
        }

        private void AssignInformation(InteractableHostManager manager) {
            List<Grip> grips = new List<Grip>();
            List<Rigidbody> rigidbodies = new List<Rigidbody>();
            foreach (var host in manager.hosts) {
                grips.AddRange(host._grips.ToArray());
                rigidbodies.Add(host.Rb);
            }

            PropGrips = grips.ToArray();
            Rigidbodies = rigidbodies.ToArray();
        }

        private void AssignInformation(GameObject go) {
            PropGrips = new Grip[0];
            Rigidbodies = go.GetComponentsInChildren<Rigidbody>(true);
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

            if (_grabbedGrips.ContainsKey(grip))
                _grabbedGrips[grip]++;
            else
                _grabbedGrips.Add(grip, 1);
        }

        public void OnDetach(Hand hand, Grip grip) {
            OnTransferOwner(hand);

            if (_grabbedGrips.ContainsKey(grip))
                _grabbedGrips[grip]--;
        }

        public void Cleanup() {
            if (!GameObject.IsNOC()) {
                Cache.Remove(GameObject);
            }

            ResetJointDrives();

            foreach (var pair in ConfigurableJoints) {
                if (!pair.Key.IsNOC()) {
                    JointCache.Remove(pair.Key);
                }
            }

            if (!WeaponSlot.IsNOC())
                WeaponSlotCache.Remove(WeaponSlot);
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
            Owner = owner;

            if (owner == PlayerIdManager.LocalSmallId) {
                ResetJointDrives();
            }
            else {
                ClearJointDrives();
            }
        }

        public void ResetJointDrives() {
            JointExtensions.IgnoreDriveCheck = true;

            foreach (var pair in ConfigurableJoints) {
                if (!pair.Key.IsNOC()) {
                    pair.Value.CopyTo(pair.Key);
                }
            }

            JointExtensions.IgnoreDriveCheck = false;
        }

        public void ClearJointDrives() {
            JointExtensions.IgnoreDriveCheck = true;

            foreach (var joint in ConfigurableJoints.Keys) {
                if (!joint.IsNOC())  {
                    joint.xDrive = new JointDrive();
                    joint.yDrive = new JointDrive();
                    joint.zDrive = new JointDrive();

                    joint.angularXDrive = new JointDrive();
                    joint.angularYZDrive = new JointDrive();
                    joint.slerpDrive = new JointDrive();
                }
            }

            JointExtensions.IgnoreDriveCheck = false;
        }

        public void VerifyOwner() {
            if (Owner.HasValue && PlayerIdManager.GetPlayerId(Owner.Value) == null)
                Owner = null;
        }

        public void VerifyID() {
            bool mismatchId = !SyncManager.Syncables.ContainsKey(Id) || SyncManager.Syncables[Id] != this;

            if (SyncManager.Syncables.ContainsValue(this) && mismatchId) {
                foreach (var pair in SyncManager.Syncables) {
                    if (pair.Value == this)
                        Id = pair.Key;
                }
            }
        }

        public void VerifyRigidbodies() {
            if (_verifyRigidbodies) {
                // Check if any are missing
                bool needToUpdate = false;
                foreach (var rb in Rigidbodies) {
                    if (rb == null) {
                        needToUpdate = true;
                        break;
                    }
                }

                // Re-get all rigidbodies
                if (needToUpdate) {
                    for (var i = 0; i < HostGameObjects.Length; i++) {
                        var host = HostGameObjects[i];

                        if (host != null)
                            Rigidbodies[i] = host.GetComponent<Rigidbody>();
                    }
                }

                _verifyRigidbodies = false;
            }
        }

        public void OnRegister(ushort id) {
            Id = id;
            _hasRegistered = true;
        }

        public ushort GetId() {
            return Id;
        }

        public byte? GetIndex(Grip grip)
        {
            for (byte i = 0; i < PropGrips.Length; i++)
            {
                if (PropGrips[i] == grip)
                    return i;
            }
            return null;
        }

        public bool IsQueued() {
            return SyncManager.QueuedSyncables.ContainsValue(this);
        }

        public bool IsRegistered() => _hasRegistered;

        private bool HasValidParameters() => !(LevelWarehouseUtilities.IsLoading() || GameObject.IsNOC() || !GameObject.activeInHierarchy || !_hasRegistered);

        public void OnFixedUpdate() {
            if (!HasValidParameters())
                return;

            if (Owner.HasValue && Owner != PlayerIdManager.LocalSmallId) {
                OnReceivedUpdate();
            }
        }

        public void OnUpdate()
        {
            if (!HasValidParameters())
                return;

            VerifyID();
            VerifyOwner();
            VerifyRigidbodies();

            if (Owner.HasValue && Owner == PlayerIdManager.LocalSmallId) {
                OnOwnedUpdate();
            }
        }

        private bool HasMoved(int index) {
            var transform = Rigidbodies[index].transform;
            var lastPosition = LastSentPositions[index];
            var lastRotation = LastSentRotations[index];

            return (transform.position - lastPosition).sqrMagnitude > 0.001f || Quaternion.Angle(transform.rotation, lastRotation) > 0.05f; 
        }

        private void OnOwnedUpdate() {
            for (var i = 0; i < Rigidbodies.Length; i++) {
                DesiredPositions[i] = null;
                DesiredRotations[i] = null;
                DesiredVelocity = 0f;
            }

            bool hasMovingBody = false;

            for (var i = 0; i < Rigidbodies.Length; i++) {
                var rb = Rigidbodies[i];

                if (rb.IsNOC()) {
                    continue;
                }

                if (!hasMovingBody && !rb.IsSleeping() && HasMoved(i)) {
                    hasMovingBody = true;
                }
            }

            if (!hasMovingBody)
                return;

            for (var i = 0; i < Rigidbodies.Length; i++) {
                var rb = Rigidbodies[i];

                if (rb.IsNOC())
                    continue;

                LastSentPositions[i] = rb.transform.position;
                LastSentRotations[i] = rb.transform.rotation;
            }

            using (var writer = FusionWriter.Create()) {
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

            bool isSomethingGrabbed = false;
            foreach (var pair in _grabbedGrips) {
                if (!pair.Key.IsNOC() && pair.Value > 0) {
                    isSomethingGrabbed = true;
                    break;
                }
            }

            if (!isSomethingGrabbed && Time.timeSinceLevelLoad - TimeOfMessage >= 1f) {
                foreach (var rb in Rigidbodies) {
                    if (!rb.IsNOC() && !rb.IsSleeping())
                        rb.Sleep();
                }

                return;
            }

            float dt = Time.fixedDeltaTime;
            float invDt = 1f / dt;

            for (var i = 0; i < Rigidbodies.Length; i++) {
                var rb = Rigidbodies[i];
                if (rb.IsNOC() || rb.IsSleeping())
                    continue;

                bool isGrabbed = false;

                foreach (var pair in _grabbedGrips) {
                    if (pair.Value > 0 && pair.Key.Host.Rb == rb) {
                        DesiredPositions[i] = null;
                        DesiredRotations[i] = null;
                        DesiredVelocity = 0f;

                        isGrabbed = true;
                        break;
                    }
                }

                if (isGrabbed || !DesiredPositions[i].HasValue || !DesiredRotations[i].HasValue)
                    continue;

                var pos = DesiredPositions[i].Value;
                var rot = DesiredRotations[i].Value;

                bool allowPosition = !IsRotationBased || i == 0;

                // Teleport check
                float distSqr = (rb.transform.position - pos).sqrMagnitude;
                if (distSqr > (2f * (DesiredVelocity + 1f)) && allowPosition) {
                    rb.transform.position = pos;
                    rb.transform.rotation = rot;

                    rb.velocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }
                // Instead calculate velocity stuff
                else {
                    if (allowPosition) {
                        var outputVel = (pos - rb.transform.position) * invDt * (IsRotationBased ? 0.1f : PropPinMlp);
                        if (!outputVel.IsNanOrInf())
                            rb.velocity = outputVel;
                    }

                    var outputAngVel = PhysXUtils.GetAngularVelocity(rb.transform.rotation, rot) * PropPinMlp;
                    if (!outputAngVel.IsNanOrInf())
                        rb.angularVelocity = outputAngVel;
                }
            }
        }
    }
}
