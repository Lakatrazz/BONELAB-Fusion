#if MELONLOADER
using Il2CppInterop.Runtime.Attributes;

using LabFusion.Network;
using LabFusion.Player;

using MelonLoader;
#endif

namespace LabFusion.Marrow.Integration
{
#if MELONLOADER
    [RegisterTypeInIl2Cpp]
#else

#endif
    public class RPCFloat : RPCVariable
    {
#if MELONLOADER
        public RPCFloat(IntPtr intPtr) : base(intPtr) { }

        private float _latestValue = 0;

        public float GetLatestValue()
        {
            return _latestValue;
        }

        public bool SetValue(float value)
        {
            return RPCFloatSender.SetValue(this, value);
        }

        public void ReceiveValue(float value)
        {
            _latestValue = value;

            InvokeHolder();
        }

        [HideFromIl2Cpp]
        public override void CatchupPlayer(PlayerID playerID) => RPCFloatSender.CatchupValue(this, playerID);

#else
        public float GetLatestValue()
        {
            return 0f;
        }

        public bool SetValue(float value)
        {
            return false;
        }
#endif
    }
}