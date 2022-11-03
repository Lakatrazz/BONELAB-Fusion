using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LabFusion.Data;
using LabFusion.Extensions;
using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.Utilities;

using SLZ;
using SLZ.Interaction;
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

        public readonly GameObject GameObject;

        public ushort Id;

        public byte? Owner = null;

        public Vector3?[] DesiredPositions;
        public Quaternion?[] DesiredRotations;

        public Vector3?[] DesiredVelocities;

        public float TimeSinceUpdateReceived;

        private bool _verifyRigidbodies;

        private bool _hasLockedPosition = false;

        public PropSyncable(InteractableHost host) {
            GameObject = host.manager ? host.manager.gameObject : host.gameObject;

            if (host.manager)
                AssignInformation(host.manager);
            else
                AssignInformation(host);

            foreach (var grip in PropGrips) {
                grip.attachedHandDelegate += (Grip.HandDelegate)OnTransferOwner;
                grip.detachedHandDelegate += (Grip.HandDelegate)OnTransferOwner;
            }

            HostGameObjects = new GameObject[Rigidbodies.Length];
            for (var i = 0; i < Rigidbodies.Length; i++) {
                HostGameObjects[i] = Rigidbodies[i].gameObject;
            }

            DesiredPositions = new Vector3?[Rigidbodies.Length];
            DesiredRotations = new Quaternion?[Rigidbodies.Length];
            DesiredVelocities = new Vector3?[Rigidbodies.Length];

            Cache.Add(GameObject, this);
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

        public void OnTransferOwner(Hand hand) {
            // Determine the manager
            // Main player
            if (hand.manager == RigData.RigReferences.RigManager) {
                SetOwner(PlayerIdManager.LocalSmallId);
            }
            // Player rep
            else if (PlayerRep.Managers.TryGetValue(hand.manager, out var rep)) {
                SetOwner(rep.PlayerId.SmallId);
            }

            _verifyRigidbodies = true;
        }

        public void Cleanup() {
            Cache.Remove(GameObject);
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
            return SyncManager.QueuedSyncables.Contains(this);
        }

        public void OnFixedUpdate() {
            if (GameObject == null || !GameObject.active)
                return;

            VerifyOwner();
            VerifyRigidbodies();

            if (Owner.HasValue && Owner == PlayerIdManager.LocalSmallId) {
                OnOwnedUpdate();
            }
            else {
                OnReceivedUpdate();
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
                if (rb == null || rb.IsSleeping())
                    continue;

                var host = InteractableHost.Cache.Get(rb.gameObject);
                if (host && host.IsAttached) {
                    DesiredPositions[i] = null;
                    DesiredRotations[i] = null;
                    DesiredVelocities[i] = null;

                    continue;
                }

                var pos = DesiredPositions[i];
                var rot = DesiredRotations[i];
                var vel = DesiredVelocities[i];

                bool hasValues = pos != null && rot != null && vel != null;

                if (hasValues) {
                    // Move position with prediction
                    if (Time.realtimeSinceStartup - TimeSinceUpdateReceived <= 1.5f) {
                        pos += vel * dt;
                        DesiredPositions[i] = pos;

                        _hasLockedPosition = false;
                    }
                    else if (!_hasLockedPosition) {
                        pos = rb.transform.position;
                        DesiredPositions[i] = pos;

                        vel = Vector3.zero;
                        DesiredVelocities[i] = Vector3.zero;

                        _hasLockedPosition = true;
                    }

                    var outputVel = ((pos.Value - rb.transform.position) * invDt * PropPinMlp) + vel.Value;
                    var outputAngVel = PhysXUtils.GetAngularVelocity(rb.transform.rotation, rot.Value) * PropPinMlp;

                    if (!outputVel.IsNanOrInf())
                        rb.velocity = Vector3.Lerp(rb.velocity, outputVel, outputVel.sqrMagnitude + rb.velocity.sqrMagnitude + (15f * dt));

                    if (!outputAngVel.IsNanOrInf())
                        rb.angularVelocity = outputAngVel;

                    // Teleport check
                    float distSqr = (rb.transform.position - pos.Value).sqrMagnitude;
                    if (distSqr > (2f * (vel.Value.magnitude + 1f))) {
                        rb.MovePosition(pos.Value);
                        rb.MoveRotation(rot.Value);
                    }
                }
            }
        }
    }
}
