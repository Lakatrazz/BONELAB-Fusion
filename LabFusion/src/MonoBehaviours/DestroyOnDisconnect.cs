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
            MultiplayerHooking.OnDisconnect += OnDisconnect;
        }

        private void OnDestroy()
        {
            MultiplayerHooking.OnDisconnect -= OnDisconnect;
        }

        private void OnDisconnect()
        {
            GameObject.Destroy(gameObject);
        }
    }
}
