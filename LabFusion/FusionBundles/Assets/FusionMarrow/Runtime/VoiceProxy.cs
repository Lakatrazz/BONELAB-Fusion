#if MELONLOADER
using Il2CppInterop.Runtime.Attributes;

using LabFusion.Data;
using LabFusion.Network;
using LabFusion.Player;
using LabFusion.Scene;
using LabFusion.SDK.Extenders;
using LabFusion.SDK.Messages;
using LabFusion.Voice;

using MelonLoader;
#endif

using UnityEngine;

namespace LabFusion.Marrow.Integration
{
#if MELONLOADER
    [RegisterTypeInIl2Cpp]
#else
    [DisallowMultipleComponent]
#endif
    public class VoiceProxy : MonoBehaviour
    {
#if MELONLOADER
        public VoiceProxy(IntPtr intPtr) : base(intPtr) { }

        public static readonly Dictionary<int, VoiceProxy> IDToProxy = new();

        public static readonly List<VoiceProxy> Proxies = new();

        public static readonly ComponentHashTable<VoiceProxy> HashTable = new();

        private string _channel = null;
        public string Channel
        {
            get
            {
                return _channel;
            }
            set
            {
                _channel = value;
            }
        }

        private VoiceProxy _connectedProxy = null;
        public VoiceProxy ConnectedProxy
        {
            get
            {
                return _connectedProxy;
            }
            set
            {
                _connectedProxy = value;
            }
        }

        private bool _connectedToSelf = false;
        public bool ConnectedToSelf
        {
            get
            {
                return _connectedToSelf;
            }
            set
            {
                _connectedToSelf = value;
            }
        }

        private int? _inputtingPlayerID = null;
        public int? InputtingPlayerID
        {
            get
            {
                return _inputtingPlayerID;
            }
            set
            {
                _inputtingPlayerID = value;
            }
        }

        private VoiceSource _voiceSource = null;

        [HideFromIl2Cpp]
        public VoiceSource VoiceSource => _voiceSource;

        private void Awake()
        {
            HashTable.AddComponent(GameObjectHasher.GetHierarchyHash(gameObject), this);

            Proxies.Add(this);

            if (NetworkSceneManager.IsLevelNetworked)
            {
                _voiceSource = VoiceSourceManager.CreateVoiceSource(gameObject, -1);
            }
        }

        private void OnDestroy()
        {
            HashTable.RemoveComponent(this);

            Proxies.RemoveAll(p => p == this);
        }

        public void SetChannelString(string channel)
        {
            Channel = channel;
        }

        public void SetChannelInt32(int channel)
        {
            Channel = channel.ToString();
        }

        public void SetConnectedProxy(VoiceProxy connectedProxy)
        {
            ConnectedProxy = connectedProxy;
        }

        public void SetConnectedToSelf(bool connectedToSelf)
        {
            ConnectedToSelf = connectedToSelf;
        }

        public void ToggleInput(bool input)
        {
            if (!NetworkSceneManager.IsLevelNetworked)
            {
                return;
            }

            SendInput(input, PlayerIDManager.LocalSmallID);
        }

        [HideFromIl2Cpp]
        private ComponentPathData CreatePathData()
        {
            return ComponentPathData.CreateFromComponent<VoiceProxy, VoiceProxyExtender>(this, HashTable, VoiceProxyExtender.Cache);
        }

        [HideFromIl2Cpp]
        private void SendInput(bool input, byte playerID)
        {
            var data = new VoiceProxyInputData()
            {
                ComponentData = CreatePathData(),
                PlayerID = playerID,
                Input = input,
            };

            MessageRelay.RelayModule<VoiceProxyInputMessage, VoiceProxyInputData>(data, NetworkChannel.Reliable, RelayType.ToClients);
        }

        [HideFromIl2Cpp]
        private void SendChannel(string channel)
        {

        }
#else
        public string DefaultChannel = null;

        public VoiceProxy DefaultConnectedProxy = null;

        public bool ConnectedToSelf = false;

        public void SetChannelString(string channel)
        {
        }

        public void SetChannelInt32(int channel)
        {
        }

        public void SetConnectedProxy(VoiceProxy connectedProxy)
        {
        }

        public void SetConnectedToSelf(bool connectedToSelf)
        {
        }

        public void ToggleInput(bool input)
        {
        }
#endif
    }
}