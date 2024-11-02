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
    public class RPCInt : RPCVariable
    {
#if MELONLOADER
        public RPCInt(IntPtr intPtr) : base(intPtr) { }

        private int _latestValue = 0;

        public int GetLatestValue()
        {
            return _latestValue;
        }

        public bool SetValue(int value)
        {
            return RPCIntSender.SetValue(this, value);
        }

        public void ReceiveValue(int value)
        {
            _latestValue = value;

            InvokeHolder();
        }
#else
        public int GetLatestValue()
        {
            return 0;
        }

        public bool SetValue(int value)
        {
            return false;
        }
#endif
    }
}