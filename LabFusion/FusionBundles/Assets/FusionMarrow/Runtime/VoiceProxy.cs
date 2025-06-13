#if MELONLOADER
using Il2CppInterop.Runtime.Attributes;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

using LabFusion.Data;
using LabFusion.Network;
using LabFusion.Player;
using LabFusion.Scene;
using LabFusion.SDK.Extenders;
using LabFusion.SDK.Messages;
using LabFusion.Utilities;
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
                if (_channel == value)
                {
                    return;
                }

                _channel = value;

                ProcessVoiceProxies();
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
                if (_connectedProxy == value)
                {
                    return;
                }

                _connectedProxy = value;

                ProcessVoiceProxies();
            }
        }

        private bool _canHearSelf = false;
        public bool CanHearSelf
        {
            get
            {
                return _canHearSelf;
            }
            set
            {
                if (_canHearSelf == value)
                {
                    return;
                }

                _canHearSelf = value;

                if (!value && VoiceSource != null && VoiceSource.ID == PlayerIDManager.LocalSmallID)
                {
                    VoiceSource.ID = -1;
                }
            }
        }

        private int? _inputID = null;
        public int? InputID
        {
            get
            {
                return _inputID;
            }
            set
            {
                if (_inputID == value)
                {
                    return;
                }

                _inputID = value;

                ProcessVoiceProxies();
            }
        }

        private VoiceSource _voiceSource = null;

        [HideFromIl2Cpp]
        public VoiceSource VoiceSource => _voiceSource;

        public bool Enabled { get; private set; } = false;

        private bool _hasNetworkEntity = false;
        public bool HasNetworkEntity
        {
            get
            {
                return _hasNetworkEntity;
            }
            set
            {
                if (_hasNetworkEntity == value)
                {
                    return;
                }

                _hasNetworkEntity = value;

                ProcessVoiceProxies();
            }
        }

        private void Awake()
        {
            HashTable.AddComponent(GameObjectHasher.GetHierarchyHash(gameObject), this);

            Proxies.Add(this);

            if (NetworkSceneManager.IsLevelNetworked)
            {
                _voiceSource = VoiceSourceManager.CreateVoiceSource(gameObject, -1);
                _voiceSource.OverrideFilter = true;
            }
        }

        private void OnDestroy()
        {
            HashTable.RemoveComponent(this);

            Proxies.RemoveAll(p => p == this);
        }

        private void OnEnable()
        {
            Enabled = true;
        }

        private void OnDisable()
        {
            Enabled = false;
        }

        private void OnAudioFilterRead(Il2CppStructArray<float> data, int channels)
        {
            if (VoiceSource == null || !VoiceSource.Playing)
            {
                return;
            }

            VoiceSource.StreamFilter.ProcessAudioFilter(data, channels);
        }

        public void SetChannelString(string channel)
        {
            Channel = channel;
        }

        public void SetChannelInt32(int channel)
        {
            Channel = channel.ToString();
        }

        public void SetConnectedProxy(UnityEngine.Object connectedProxy)
        {
            ConnectedProxy = connectedProxy.TryCast<VoiceProxy>();
        }

        public void SetCanHearSelf(bool value)
        {
            CanHearSelf = value;
        }

        public float GetOutputAmplitude() => VoiceSource != null ? VoiceSource.Amplitude : 0f;

        public bool IsOutputtingVoice() => VoiceSource != null && VoiceSource.ID != -1 && VoiceSource.ReceivingInput;

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

            MessageRelay.RelayModule<VoiceProxyInputMessage, VoiceProxyInputData>(data, CommonMessageRoutes.ReliableToClients);
        }

        private static void ProcessVoiceProxies()
        {
            if (!NetworkSceneManager.IsLevelNetworked)
            {
                return;
            }

            foreach (var proxy in Proxies)
            {
                try
                {
                    proxy.ProcessVoiceProxy();
                }
                catch (Exception e)
                {
                    FusionLogger.LogException("processing VoiceProxy", e);
                }
            }
        }

        private void ProcessVoiceProxy()
        {
            VoiceSource.ID = -1;

            VoiceProxy listeningProxy = null;

            if (ConnectedProxy != null)
            {
                listeningProxy = ConnectedProxy;
            }
            else if (!string.IsNullOrWhiteSpace(Channel))
            {
                listeningProxy = Proxies.Find(ProxyChannelPredicate);
            }

            if (listeningProxy != null)
            {
                int inputID = listeningProxy.InputID ?? -1;

                if (inputID == PlayerIDManager.LocalSmallID && !CanHearSelf)
                {
                    inputID = -1;
                }

                VoiceSource.ID = inputID;
            }
        }

        [HideFromIl2Cpp]
        private bool ProxyChannelPredicate(VoiceProxy proxy)
        {
            if (proxy == this)
            {
                return false;
            }

            if (proxy.Channel != Channel)
            {
                return false;
            }

            if (proxy.ConnectedProxy != null)
            {
                return false;
            }

            if (!proxy.HasNetworkEntity && !proxy.Enabled)
            {
                return false;
            }

            if (!proxy.InputID.HasValue)
            {
                return false;
            }

            return true;
        }
#else
        [Tooltip("The default channel that this VoiceProxy will listen for. If another VoiceProxy with the same channel is receiving input, then it will play back.")]
        public string DefaultChannel = null;

        [Tooltip("A specific VoiceProxy that this VoiceProxy will listen for. If the connected proxy is receiving input, then it will play back.")]
        public VoiceProxy DefaultConnectedProxy = null;

        [Tooltip("Whether or not this VoiceProxy is able to play the Local Player's voice back to them.")]
        public bool CanHearSelf = false;

        public void SetChannelString(string channel)
        {
        }

        public void SetChannelInt32(int channel)
        {
        }

        public void SetConnectedProxy(UnityEngine.Object connectedProxy)
        {
        }

        public void SetCanHearSelf(bool value)
        {
        }

        public float GetOutputAmplitude() => 0f;

        public bool IsOutputtingVoice() => false;

        public void ToggleInput(bool input)
        {
        }
#endif
    }
}