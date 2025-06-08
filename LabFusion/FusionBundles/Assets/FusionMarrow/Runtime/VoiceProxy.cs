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
                _inputID = value;

                ProcessVoiceProxies();
            }
        }

        private VoiceSource _voiceSource = null;

        [HideFromIl2Cpp]
        public VoiceSource VoiceSource => _voiceSource;

        private void Awake()
        {
            HashTable.AddComponent(GameObjectHasher.GetHierarchyHash(gameObject), this);

            if (NetworkSceneManager.IsLevelNetworked)
            {
                _voiceSource = VoiceSourceManager.CreateVoiceSource(gameObject, -1);
                _voiceSource.OverrideFilter = true;
            }
        }

        private void OnDestroy()
        {
            HashTable.RemoveComponent(this);
        }

        private void OnEnable()
        {
            Proxies.Add(this);
        }

        private void OnDisable()
        {
            Proxies.RemoveAll(p => p == this);
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

        public void SetConnectedProxy(VoiceProxy connectedProxy)
        {
            ConnectedProxy = connectedProxy;
        }

        public void SetCanHearSelf(bool value)
        {
            CanHearSelf = value;
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
                listeningProxy = Proxies.Find(p => p != this && p.Channel == Channel && !p.ConnectedProxy);
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

        public void SetConnectedProxy(VoiceProxy connectedProxy)
        {
        }

        public void SetCanHearSelf(bool value)
        {
        }

        public void ToggleInput(bool input)
        {
        }
#endif
    }
}