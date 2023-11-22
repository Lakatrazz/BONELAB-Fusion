using System;
using LabFusion.Utilities;
using MelonLoader;
using UnityEngine;

namespace LabFusion.MonoBehaviours
{
    [RegisterTypeInIl2Cpp]
    public class DestroyOnDisconnect : MonoBehaviour
    {
        public DestroyOnDisconnect(IntPtr intPtr) : base(intPtr) { }

        private void Awake()
        {
            MultiplayerHooking.OnDisconnect += OnDisconnect;
        }

        private void OnDestroy()
        {
            MultiplayerHooking.OnDisconnect -= OnDisconnect;
        }

        private void OnDisconnect()
        {
            Destroy(gameObject);
        }
    }
}
