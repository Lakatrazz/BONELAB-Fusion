#if MELONLOADER
using LabFusion.Network;

using MelonLoader;
#endif

namespace LabFusion.Marrow.Integration
{
#if MELONLOADER
    [RegisterTypeInIl2Cpp]
#else

#endif
    public class RPCBool : RPCVariable
    {
#if MELONLOADER
        public RPCBool(IntPtr intPtr) : base(intPtr) { }

        private bool _latestValue = false;

        public bool GetLatestValue()
        {
            return _latestValue;
        }

        public bool SetValue(bool value)
        {
            return RPCBoolSender.SetValue(this, value);
        }

        public void ReceiveValue(bool value)
        {
            _latestValue = value;

            InvokeHolder();
        }
#else
        public bool GetLatestValue()
        {
            return false;
        }

        public bool SetValue(bool value)
        {
            return false;
        }
#endif
    }
}