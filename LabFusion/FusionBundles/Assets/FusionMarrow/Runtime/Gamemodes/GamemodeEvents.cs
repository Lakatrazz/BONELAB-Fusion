using UnityEngine;

#if MELONLOADER
using MelonLoader;

using Il2CppUltEvents;

using Il2CppInterop.Runtime.InteropTypes.Fields;
using Il2CppInterop.Runtime.Attributes;

using LabFusion.SDK.Gamemodes;
#else
using UltEvents;
#endif

namespace LabFusion.Marrow.Integration
{
#if MELONLOADER
    [RegisterTypeInIl2Cpp]
#endif
    public class GamemodeEvents : MonoBehaviour
    {
#if MELONLOADER
        public GamemodeEvents(IntPtr intPtr) : base(intPtr) { }

        public Il2CppReferenceField<UltEventHolder> onGamemodeStartedHolder;

        public Il2CppReferenceField<UltEventHolder> onGamemodeEndedHolder;

        private void Awake()
        {
            GamemodeManager.OnGamemodeChanged += OnGamemodeChanged;
        }

        private void OnDestroy()
        {
            GamemodeManager.OnGamemodeChanged -= OnGamemodeChanged;
        }

        [HideFromIl2Cpp]
        private void OnGamemodeChanged(Gamemode gamemode)
        {
            if (gamemode != null)
            {
                onGamemodeStartedHolder.Get()?.Invoke();
            }
            else 
            {
                onGamemodeEndedHolder.Get()?.Invoke();
            }
        }
#else
        public UltEventHolder onGamemodeStartedHolder;

        public UltEventHolder onGamemodeEndedHolder;
#endif
    }
}