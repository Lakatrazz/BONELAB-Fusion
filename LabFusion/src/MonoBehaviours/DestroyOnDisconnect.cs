using UnityEngine;

using MelonLoader;
using LabFusion.Utilities;

namespace LabFusion.MonoBehaviours
{
    [RegisterTypeInIl2Cpp]
    public class DestroyOnDisconnect : MonoBehaviour
    {
        public DestroyOnDisconnect(IntPtr intPtr) : base(intPtr) { }

        private void Awake()
        {
            MultiplayerHooking.OnDisconnected += OnDisconnect;
        }

        private void OnDestroy()
        {
            MultiplayerHooking.OnDisconnected -= OnDisconnect;
        }

        private void OnDisconnect()
        {
            GameObject.Destroy(gameObject);
        }
    }
}
