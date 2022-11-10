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

using SLZ;
using SLZ.Interaction;
using SLZ.Marrow.Pool;
using SLZ.Utilities;

using UnityEngine;

namespace LabFusion.Syncables
{
    public class PropSyncable : ISyncable {
        public static readonly Dictionary<GameObject, PropSyncable> Cache = new Dictionary<GameObject, PropSyncable>(new UnityComparer());

        public const float PropPinMlp = 0.8f;

        public Grip[] PropGrips;
        public Rigidbody[] Rigidbodies;
        public GameObject[] HostGameObjects;

        public readonly AssetPoolee AssetPoolee;

        public readonly GameObject GameObject;

        public ushort Id;

        public byte? Owner = null;

        public Vector3?[] DesiredPositions;
        public Quaternion?[] DesiredRotations;

        public Vector3?[] DesiredVelocities;

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
            for (var i = 0; i < Rigidbodies.Length; i++) {
                HostGameObjects[i] = Rigidbodies[i].gameObject;
            }

            DesiredPositions = new Vector3?[Rigidbodies.Length];
            DesiredRotations = new Quaternion?[Rigidbodies.Length];
            DesiredVelocities = new Vector3?[Rigidbodies.Length];
        }

        private void AssignInformation(InteractableHost host) {
            var hosts = host.GetComponentsInChildren<InteractableHost>(true);

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

        private bool HasValidParameters() => !(GameObject.IsNOC() || !GameObject.active || !_hasRegistered);

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

        private void OnOwnedUpdate() {
            for (var i = 0; i < Rigidbodies.Length; i++) {
                DesiredPositions[i] = null;
                DesiredRotations[i] = null;
                DesiredVelocities[i] = null;
            }

            foreach (var rb in Rigidbodies)
                if (rb != null && rb.IsSleeping()) return;

            using (var writer = FusionWriter.Create()) {
                using (var data = PropSyncableUpdateData.Create(PlayerIdManager.LocalSmallId, Id, HostGameObjects, Rigidbodies)) {
                    writer.Write(data);

                    using (var message = FusionMessage.Create(NativeMessageTag.PropSyncableUpdate, writer)) {
                        MessageSender.BroadcastMessage(NetworkChannel.Unreliable, message);
                    }
                }
            }
        }

        private void OnReceivedUpdate() {
            if (!SafetyUtilities.IsValidTime)
                return;

            float dt = Time.fixedDeltaTime;
            float invDt = 1f / dt;

            for (var i = 0; i < Rigidbodies.Length; i++) {
                var rb = Rigidbodies[i];
                if (rb.IsNOC() || rb.ShouldSleep())
                    continue;

                bool isGrabbed = false;

                foreach (var pair in _grabbedGrips) {
                    if (pair.Value > 0 && pair.Key.Host.Rb == rb) {
                        DesiredPositions[i] = null;
                        DesiredRotations[i] = null;
                        DesiredVelocities[i] = null;

                        isGrabbed = true;
                        break;
                    }
                }

                if (isGrabbed || !DesiredPositions[i].HasValue || !DesiredRotations[i].HasValue || !DesiredVelocities[i].HasValue)
                    continue;

                var pos = DesiredPositions[i].Value;
                var rot = DesiredRotations[i].Value;
                var vel = DesiredVelocities[i].Value;

                var outputVel = (pos - rb.transform.position) * invDt * PropPinMlp;
                var outputAngVel = PhysXUtils.GetAngularVelocity(rb.transform.rotation, rot) * PropPinMlp;

                if (!outputVel.IsNanOrInf())
                    rb.velocity = outputVel;

                if (!outputAngVel.IsNanOrInf())
                    rb.angularVelocity = outputAngVel;

                // Teleport check
                float distSqr = (rb.transform.position - pos).sqrMagnitude;
                if (distSqr > (2f * (vel.sqrMagnitude + 1f))) {
                    rb.transform.position = pos;
                    rb.transform.rotation = rot;
                }
            }
        }
    }
}
