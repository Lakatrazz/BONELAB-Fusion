using Il2CppInterop.Runtime.Attributes;

using LabFusion.Network;
using LabFusion.Utilities;

using MelonLoader;

using UnityEngine;

namespace LabFusion.MonoBehaviours
{
    [RegisterTypeInIl2Cpp]
    public class MirrorIdentifier : MonoBehaviour
    {
        public MirrorIdentifier(IntPtr intPtr) : base(intPtr) { }

        [HideFromIl2Cpp]
        public ClientSmallID ID { get; set; }

        public void Awake()
        {
            MultiplayerHooking.OnDisconnected += OnDisconnect;
        }

        public void OnDestroy()
        {
            MultiplayerHooking.OnDisconnected -= OnDisconnect;
        }

        private void OnDisconnect()
        {
            GameObject.Destroy(this);
        }
    }
}
