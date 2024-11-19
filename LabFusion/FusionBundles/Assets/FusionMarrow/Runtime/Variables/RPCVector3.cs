#if MELONLOADER
using LabFusion.Network;

using MelonLoader;
#endif

using UnityEngine;

namespace LabFusion.Marrow.Integration
{
#if MELONLOADER
    [RegisterTypeInIl2Cpp]
#else

#endif
    public class RPCVector3 : RPCVariable
    {
#if MELONLOADER
        public RPCVector3(IntPtr intPtr) : base(intPtr) { }

        private Vector3 _latestValue = Vector3.zero;

        public Vector3 GetLatestValue()
        {
            return _latestValue;
        }

        public bool SetValue(Vector3 value)
        {
            return RPCVector3Sender.SetValue(this, value);
        }

        public void ReceiveValue(Vector3 value)
        {
            _latestValue = value;

            InvokeHolder();
        }
#else
        public Vector3 GetLatestValue()
        {
            return Vector3.zero;
        }

        public bool SetValue(Vector3 value)
        {
            return false;
        }
#endif
    }
}