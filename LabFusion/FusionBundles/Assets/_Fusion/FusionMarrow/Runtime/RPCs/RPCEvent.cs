using System.Collections;
using System.Collections.Generic;

#if MELONLOADER
using Il2CppInterop.Runtime.InteropTypes.Fields;

using Il2CppUltEvents;

using LabFusion.Data;
using LabFusion.Network;

using MelonLoader;
#else
using UltEvents;
#endif

using UnityEngine;

namespace LabFusion.Marrow.Integration
{
#if MELONLOADER
    [RegisterTypeInIl2Cpp]
#else
    [HelpURL("https://github.com/Lakatrazz/BONELAB-Fusion/wiki/Remote-Procedure-Calls#rpc-event")]
#endif
    public class RPCEvent : MonoBehaviour
    {
        public enum RPCRelayType
        {
            ToServer,
            ToClients,
            ToOtherClients
        }

        public enum RPCChannel
        {
            Reliable = 0,
            Unreliable = 1,
        }

#if MELONLOADER
        public RPCEvent(IntPtr intPtr) : base(intPtr) { }

        public static readonly ComponentHashTable<RPCEvent> HashTable = new();

        public Il2CppValueField<int> relayType;

        public Il2CppValueField<int> channel;

        public Il2CppValueField<bool> requiresOwnership;

        public Il2CppReferenceField<UltEventHolder> onEventReceivedHolder;

        private int _relayTypeCached;
        private int _channelCached;
        private bool _requiresOwnershipCached;

        public RPCRelayType RelayType => (RPCRelayType)_relayTypeCached;
        public RPCChannel Channel => (RPCChannel)_channelCached;
        public bool RequiresOwnership => _requiresOwnershipCached;

        private void Awake()
        {
            _relayTypeCached = relayType.Get();
            _channelCached = channel.Get();
            _requiresOwnershipCached = requiresOwnership.Get();

            var hash = GameObjectHasher.GetHierarchyHash(gameObject);

            HashTable.AddComponent(hash, this);
        }

        private void OnDestroy()
        {
            HashTable.RemoveComponent(this);
        }

        public void Receive()
        {
            onEventReceivedHolder.Get()?.Invoke();
        }

        public bool Invoke()
        {
            return RPCEventSender.Invoke(this);
        }
#else
        public RPCRelayType relayType = RPCRelayType.ToServer;

        public RPCChannel channel = RPCChannel.Reliable;

        public bool requiresOwnership = false;

        public UltEventHolder onEventReceivedHolder;

        public bool Invoke()
        {
            return false;
        }
#endif
    }
}