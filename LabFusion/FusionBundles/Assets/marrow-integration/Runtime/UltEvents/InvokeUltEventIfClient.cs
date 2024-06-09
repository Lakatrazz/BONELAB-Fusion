#if MELONLOADER
using MelonLoader;

using LabFusion.Network;

using Il2CppUltEvents;
#else
using UltEvents;
using UnityEngine;
#endif

namespace LabFusion.MarrowIntegration
{
#if MELONLOADER
    [RegisterTypeInIl2Cpp]
#else
    [AddComponentMenu("BONELAB Fusion/UltEvents/Invoke Ult Event If Client")]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(UltEventHolder))]
#endif
    public sealed class InvokeUltEventIfClient : FusionMarrowBehaviour
    {
#if MELONLOADER
        public InvokeUltEventIfClient(IntPtr intPtr) : base(intPtr) { }

        private void Start()
        {
            var holder = GetComponent<UltEventHolder>();

            if (NetworkInfo.IsClient && holder != null)
                holder.Invoke();
        }
#else
        public override string Comment => "The UltEventHolder attached to this GameObject will be executed on level load if the player is a client and not a server.";
#endif
    }
}
