#if MELONLOADER
using MelonLoader;
#endif

using UnityEngine;

namespace LabFusion.Marrow.Integration
{
#if MELONLOADER
    [RegisterTypeInIl2Cpp]
#else
    [RequireComponent(typeof(AudioSource))]
#endif
    public class VoiceProxy : MonoBehaviour
    {
#if MELONLOADER
        public VoiceProxy(IntPtr intPtr) : base(intPtr) { }

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
#else
        public void SetChannelString(string channel)
        {
        }

        public void SetChannelInt32(int channel)
        {
        }

        public void SetConnectedProxy(VoiceProxy connectedProxy)
        {
        }
#endif
    }
}