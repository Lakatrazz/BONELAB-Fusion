using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.Xml;
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
using SLZ.AI;
using SLZ.Interaction;
using SLZ.Marrow.Pool;
using SLZ.Marrow.SceneStreaming;
using SLZ.Props.Weapons;
using SLZ.Utilities;
using SLZ.Vehicle;
using UnityEngine;

namespace LabFusion.Syncables
{
    public class PropSyncable : ISyncable {
        public static readonly Dictionary<GameObject, PropSyncable> Cache = new Dictionary<GameObject, PropSyncable>(new UnityComparer());
        public static readonly Dictionary<WeaponSlot, PropSyncable> WeaponSlotCache = new Dictionary<WeaponSlot, PropSyncable>(new UnityComparer());

        public static readonly Dictionary<Magazine, PropSyncable> MagazineCache = new Dictionary<Magazine, PropSyncable>(new UnityComparer());
        public static readonly Dictionary<Gun, PropSyncable> GunCache = new Dictionary<Gun, PropSyncable>(new UnityComparer());

        public static readonly Dictionary<Seat, PropSyncable> SeatCache = new Dictionary<Seat, PropSyncable>(new UnityComparer());

        public static readonly Dictionary<PuppetMaster, PropSyncable> PuppetMasterCache = new Dictionary<PuppetMaster, PropSyncable>(new UnityComparer());

        public static readonly Dictionary<SimpleGripEvents, PropSyncable> SimpleGripEventsCache = new Dictionary<SimpleGripEvents, PropSyncable>(new UnityComparer());

        public PuppetMaster PuppetMaster;
        public AIBrain AIBrain;

        public Grip[] PropGrips;
        public SimpleGripEvents[] SimpleGripEvents;
        public Rigidbody[] Rigidbodies;
        public RigidbodyState[] RigidbodyStates;
        public PDController[] PDControllers;
        public GameObject[] HostGameObjects;

        public readonly AssetPoolee AssetPoolee;
        public readonly WeaponSlot WeaponSlot;

        public readonly Seat[] Seats;

        public readonly Magazine Magazine;
        public readonly Gun Gun;
        public readonly AmmoSocket AmmoSocket;

        public readonly GameObject GameObject;

        public readonly bool IsRotationBased;

        public readonly bool HasIgnoreHierarchy;

        public float TimeOfMessage = 0f;

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
            RigidbodyStates = new RigidbodyState[Rigidbodies.Length];
            PDControllers = new PDController[Rigidbodies.Length];

            for (var i = 0; i < Rigidbodies.Length; i++) {
                HostGameObjects[i] = Rigidbodies[i].gameObject;
                RigidbodyStates[i] = new RigidbodyState(Rigidbodies[i]);

                PDControllers[i] = new PDController();
                PDControllers[i].OnResetDerivatives(Rigidbodies[i]);
            }

            DesiredPositions = new Vector3?[Rigidbodies.Length];
            DesiredRotations = new Quaternion?[Rigidbodies.Length];
            DesiredVelocity = 0f;

            LastSentPositions = new Vector3[Rigidbodies.Length];
            LastSentRotations = new Quaternion[Rigidbodies.Length];

            WeaponSlot = GameObject.GetComponentInChildren<WeaponSlot>(true);

            if (WeaponSlot) {
                WeaponSlotCache.Remove(WeaponSlot);
                WeaponSlotCache.Add(WeaponSlot, this);
            }

            Magazine = GameObject.GetComponentInChildren<Magazine>(true);

            if (Magazine) {
                MagazineCache.Remove(Magazine);
                MagazineCache.Add(Magazine, this);
            }

            Gun = GameObject.GetComponentInChildren<Gun>(true);

            if (Gun) {
                GunCache.Remove(Gun);
                GunCache.Add(Gun, this);
            }

            AmmoSocket = GameObject.GetComponentInChildren<AmmoSocket>(true);

            PuppetMaster = GameObject.GetComponentInChildren<PuppetMaster>(true);

            if (PuppetMaster)
                PuppetMasterCache.Add(PuppetMaster, this);

            AIBrain = GameObject.GetComponentInChildren<AIBrain>(true);

            Seats = GameObject.GetComponentsInChildren<Seat>(true);

            foreach (var seat in Seats) {
                SeatCache.Add(seat, this);
            }

            HasIgnoreHierarchy = GameObject.GetComponentInParent<IgnoreHierarchy>(true);

            SimpleGripEvents = GameObject.GetComponentsInChildren<SimpleGripEvents>(true);

            foreach (var events in SimpleGripEvents) {
                SimpleGripEventsCache.Add(events, this);
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

        public byte? GetIndex(SimpleGripEvents gripEvents)
        {
            for (byte i = 0; i < SimpleGripEvents.Length; i++)
            {
                if (SimpleGripEvents[i] == gripEvents)
                    return i;
            }
            return null;
        }

        public SimpleGripEvents GetGripEvents(byte index)
        {
            if (SimpleGripEvents != null && SimpleGripEvents.Length > index)
                return SimpleGripEvents[index];
            return null;
        }

        public byte? GetIndex(Seat seat)
        {
            for (byte i = 0; i < Seats.Length; i++)
            {
                if (Seats[i] == seat)
                    return i;
            }
            return null;
        }

        public Seat GetSeat(byte index)
        {
            if (Seats != null && Seats.Length > index)
                return Seats[index];
            return null;
        }

        public void Cleanup() {
            if (!GameObject.IsNOC()) {
                Cache.Remove(GameObject);
            }

            if (!WeaponSlot.IsNOC())
                WeaponSlotCache.Remove(WeaponSlot);

            if (!Magazine.IsNOC())
                MagazineCache.Remove(Magazine);

            if (!Gun.IsNOC())
                GunCache.Remove(Gun);

            if (!PuppetMaster.IsNOC())
                PuppetMasterCache.Remove(PuppetMaster);

            foreach (var seat in Seats) {
                if (!seat.IsNOC())
                    SeatCache.Remove(seat);
            }

            foreach (var events in SimpleGripEvents) {
                if (!events.IsNOC())
                    SimpleGripEventsCache.Remove(events);
            }

            OnClearOverrides();
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
            OnVerifyOverrides();
        }

        public bool IsOwner() => Owner.HasValue && Owner.Value == PlayerIdManager.LocalSmallId;

        private void OnVerifyOverrides() {
            for (var i = 0; i < HostGameObjects.Length; i++) {
                var rb = Rigidbodies[i];

                // Update rigidbody states
                if (!rb.IsNOC()) {
                    // If we are the owner
                    if (IsOwner()) {
                        RigidbodyStates[i].OnClearOverride(rb);
                    }
                    // Otherwise
                    else {
                        RigidbodyStates[i].OnSetOverride(rb);
                    }
                }
            }
        }

        private void OnClearOverrides() {
            for (var i = 0; i < HostGameObjects.Length; i++) {
                var rb = Rigidbodies[i];

                // Update rigidbody states
                if (!rb.IsNOC()) {
                    RigidbodyStates[i].OnClearOverride(rb);
                }
            }
        }

        public void VerifyID()
        {
            bool mismatchId = !SyncManager.Syncables.ContainsKey(Id) || SyncManager.Syncables[Id] != this;

            if (SyncManager.Syncables.ContainsValue(this) && mismatchId)
            {
                foreach (var pair in SyncManager.Syncables)
                {
                    if (pair.Value == this)
                        Id = pair.Key;
                }
            }
        }

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

                    OnVerifyOverrides();
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

        public ushort? GetIndex(Grip grip)
        {
            for (ushort i = 0; i < PropGrips.Length; i++)
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

        private bool HasValidParameters() => !(!_hasRegistered || LevelWarehouseUtilities.IsLoading() || GameObject.IsNOC() || !GameObject.activeInHierarchy);

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

            return (transform.position - lastPosition).sqrMagnitude > 0.01f || Quaternion.Angle(transform.rotation, lastRotation) > 0.15f; 
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
                PDControllers[i].OnResetDerivatives(rb);
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
                    foreach (var hand in pair.Key.attachedHands) {
                        if (!hand.manager.activeSeat) {
                            isSomethingGrabbed = true;
                            break;
                        }
                    }
                }
            }

            if (!isSomethingGrabbed && Time.timeSinceLevelLoad - TimeOfMessage >= 1f) {
                for (var i = 0; i < Rigidbodies.Length; i++) {
                    var rb = Rigidbodies[i];

                    if (!rb.IsNOC() && !rb.IsSleeping()) {
                        rb.Sleep();
                    }
                }

                return;
            }

            for (var i = 0; i < Rigidbodies.Length; i++) {
                var rb = Rigidbodies[i];
                if (rb.IsNOC() || rb.IsSleeping())
                    continue;

                bool isGrabbed = false;

                foreach (var pair in _grabbedGrips) {
                    if (pair.Value > 0 && pair.Key.Host.Rb == rb) {
                        foreach (var hand in pair.Key.attachedHands) {
                            if (!hand.manager.activeSeat) {
                                DesiredPositions[i] = null;
                                DesiredRotations[i] = null;
                                DesiredVelocity = 0f;

                                isGrabbed = true;
                                break;
                            }
                        }

                        if (isGrabbed)
                            break;
                    }
                }

                if (isGrabbed || !DesiredPositions[i].HasValue || !DesiredRotations[i].HasValue) {
                    PDControllers[i].OnResetDerivatives(rb);
                    continue;
                }

                var pos = DesiredPositions[i].Value;
                var rot = DesiredRotations[i].Value;

                bool allowPosition = (!IsRotationBased || i == 0) && !HasIgnoreHierarchy;

                var pdController = PDControllers[i];

                // Teleport check
                float distSqr = (rb.transform.position - pos).sqrMagnitude;
                if (distSqr > (2f * (DesiredVelocity + 1f)) && allowPosition) {
                    rb.transform.position = pos;
                    rb.transform.rotation = rot;

                    rb.velocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;

                    pdController.OnResetDerivatives(rb);
                }
                // Instead calculate velocity stuff
                else {
                    if (allowPosition) {
                        rb.AddForce(pdController.GetForce(rb, pos), ForceMode.Acceleration);
                    }
                    else {
                        if (rb.useGravity)
                            rb.AddForce(-Physics.gravity, ForceMode.Acceleration);
                        pdController.OnResetPosDerivatives(rb);
                    }

                    rb.AddTorque(pdController.GetTorque(rb, rot), ForceMode.Acceleration);
                }
            }
        }
    }
}
