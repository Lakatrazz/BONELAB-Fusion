using System;
using MelonLoader;
using UnhollowerBaseLib.Attributes;
using UnityEngine;

namespace LabFusion.MonoBehaviours
{
    [RegisterTypeInIl2Cpp]
    public class NotifyOnEnable : MonoBehaviour
    {
        public NotifyOnEnable(IntPtr intPtr) : base(intPtr) { }

        private Action _hook;
        private bool _isEnabled;

        [HideFromIl2Cpp]
        public void Hook(Action hook)
        {
            if (_isEnabled)
            {
                hook();
            }
            else
            {
                _hook += hook;
            }
        }

        public void OnEnable()
        {
            _isEnabled = true;

            _hook?.Invoke();
            _hook = null;
        }

        public void OnDisable()
        {
            _isEnabled = false;
        }

        public void OnDestroy()
        {
            _hook = null;
        }
    }
}
