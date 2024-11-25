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
    public class RPCString : RPCVariable
    {
#if MELONLOADER
        public RPCString(IntPtr intPtr) : base(intPtr) { }

        private string _latestValue = string.Empty;

        public string GetLatestValue()
        {
            return _latestValue;
        }

        public bool SetValue(string value)
        {
            return RPCStringSender.SetValue(this, value);
        }

        public void ReceiveValue(string value)
        {
            _latestValue = value;

            InvokeHolder();
        }
#else
        public string GetLatestValue()
        {
            return string.Empty;
        }

        public bool SetValue(string value)
        {
            return false;
        }
#endif
    }
}