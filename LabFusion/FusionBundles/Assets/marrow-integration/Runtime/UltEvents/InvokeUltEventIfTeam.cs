using UnityEngine;

#if MELONLOADER
using MelonLoader;

using LabFusion.Utilities;

using Il2CppUltEvents;
#else
using UltEvents;
#endif

namespace LabFusion.MarrowIntegration
{
#if MELONLOADER
    [RegisterTypeInIl2Cpp]
#else
    [AddComponentMenu("BONELAB Fusion/UltEvents/Invoke Ult Event If Team")]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(UltEventHolder))]
#endif
    public sealed class InvokeUltEventIfTeam : FusionMarrowBehaviour
    {
#if MELONLOADER
        public InvokeUltEventIfTeam(IntPtr intPtr) : base(intPtr) { }

        public static readonly FusionComponentCache<GameObject, InvokeUltEventIfTeam> Cache = new FusionComponentCache<GameObject, InvokeUltEventIfTeam>();

        public string TeamName { get; private set; }

        private void Awake()
        {
            Cache.Add(gameObject, this);
        }

        private void OnDestroy()
        {
            Cache.Remove(gameObject);
        }

        public void Invoke()
        {
            var holder = GetComponent<UltEventHolder>();

            if (holder != null)
                holder.Invoke();
        }

        public void SetTeamName(string name)
        {
            TeamName = name;
        }
#else
        public override string Comment => "The UltEventHolder attached to this GameObject will be executed when the local player becomes part of a designated team in any team-based mode.";

        private string teamName;

        public void SetTeamName(string name) { }
#endif
    }
}
