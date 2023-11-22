using System;
using LabFusion.Utilities;
using MelonLoader;
using UnityEngine;

namespace LabFusion.MonoBehaviours
{
    [RegisterTypeInIl2Cpp]
    public class MirrorIdentifier : MonoBehaviour
    {
        public MirrorIdentifier(IntPtr intPtr) : base(intPtr) { }

        public byte id;

        public void Awake()
        {
            MultiplayerHooking.OnDisconnect += OnDisconnect;
        }

        public void OnDestroy()
        {
            MultiplayerHooking.OnDisconnect -= OnDisconnect;
        }

        private void OnDisconnect()
        {
            Destroy(this);
        }
    }
}
