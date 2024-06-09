using LabFusion.Utilities;

using UnityEngine;

using MelonLoader;

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
            GameObject.Destroy(this);
        }
    }
}
