#if MELONLOADER
using Il2CppInterop.Runtime.Attributes;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

using LabFusion.Data;
using LabFusion.Entities;
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
    using Math = System.Math;

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

        [HideFromIl2Cpp]
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

        public bool DistanceFalloff { get; set; } = false;

        public float FalloffMinDistance { get; set; } = 0.2f;

        public float FalloffMaxDistance { get; set; } = 500f;

        private int? _inputID = null;

        [HideFromIl2Cpp]
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

        [HideFromIl2Cpp]
        public float InputVolume { get; set; } = 1f;

        private VoiceSource _voiceSource = null;

        [HideFromIl2Cpp]
        public VoiceSource VoiceSource => _voiceSource;

        private VoiceProxy _listeningProxy = null;

        [HideFromIl2Cpp]
        public VoiceProxy ListeningProxy => _listeningProxy;

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

            InputVolume = 1f;
        }

        private void OnDisable()
        {
            Enabled = false;

            InputVolume = 1f;
        }

        private void LateUpdate()
        {
            InputVolume = CalculateInputFalloff();
        }

        private void OnAudioFilterRead(Il2CppStructArray<float> data, int channels)
        {
            if (VoiceSource == null || !VoiceSource.Playing)
            {
                return;
            }

            var streamFilter = VoiceSource.StreamFilter;

            if (ListeningProxy != null)
            {
                float inputVolume = ListeningProxy.InputVolume;
                streamFilter.SampleMultiplier = inputVolume * inputVolume;
            }
            else
            {
                streamFilter.SampleMultiplier = 1f;
            }

            streamFilter.ProcessAudioFilter(data, channels);
        }

        private float CalculateInputFalloff()
        {
            if (!DistanceFalloff)
            {
                return 1f;
            }

            if (!InputID.HasValue)
            {
                return 1f;
            }

            if (!NetworkPlayerManager.TryGetPlayer((byte)InputID.Value, out var networkPlayer) || !networkPlayer.HasRig)
            {
                return 1f;
            }

            var distance = (networkPlayer.RigRefs.Mouth.position - transform.position).magnitude;

            return CalculateVolumeFalloff(distance);
        }

        private float CalculateVolumeFalloff(float distance)
        {
            if (!DistanceFalloff)
            {
                return 1f;
            }

            float clampedMin = Math.Max(0.05f, FalloffMinDistance);
            float clampedMax = Math.Max(clampedMin + 0.05f, FalloffMaxDistance);

            float clampedDistance = Math.Max(clampedMin, distance);

            float volume = (float)Math.Pow(clampedMin / clampedDistance, 500f / clampedMax);

            return volume;
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

        public void SetDistanceFalloff(bool enabled, float min, float max)
        {
            DistanceFalloff = enabled;
            FalloffMinDistance = min;
            FalloffMaxDistance = max;
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

            _listeningProxy = null;

            if (ConnectedProxy != null)
            {
                _listeningProxy = ConnectedProxy;
            }
            else if (!string.IsNullOrWhiteSpace(Channel))
            {
                _listeningProxy = Proxies.Find(ProxyChannelPredicate);
            }

            if (ListeningProxy != null)
            {
                int inputID = ListeningProxy.InputID ?? -1;

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

        [Tooltip("When inputting voice, should voice volume falloff logarithmically with distance from the mouth to the proxy?")]
        public bool DistanceFalloff = false;

        [Min(0f)]
        [Tooltip("If distance falloff is enabled, then the voice will remain at full volume up until this distance.")]
        public float FalloffMinDistance = 0.2f;

        [Min(0f)]
        [Tooltip("If distance falloff is enabled, then the voice will become quietest at this distance.")]
        public float FalloffMaxDistance = 500f;

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

        public void SetDistanceFalloff(bool enabled, float min, float max)
        {
        }

        public float GetOutputAmplitude() => 0f;

        public bool IsOutputtingVoice() => false;

        public void ToggleInput(bool input)
        {
        }

        private void OnDrawGizmosSelected()
        {
            if (!DistanceFalloff)
            {
                return;
            }

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, FalloffMinDistance);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, FalloffMaxDistance);
        }
#endif
    }
}