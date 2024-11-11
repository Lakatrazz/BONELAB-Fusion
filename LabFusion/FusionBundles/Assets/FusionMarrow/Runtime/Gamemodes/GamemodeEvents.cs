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

        public Il2CppReferenceField<UltEventHolder> onGamemodeStoppedHolder;

        private void Awake()
        {
            GamemodeManager.OnGamemodeStarted += OnGamemodeStarted;
            GamemodeManager.OnGamemodeStopped += OnGamemodeStopped;
        }

        private void OnDestroy()
        {
            GamemodeManager.OnGamemodeStarted -= OnGamemodeStarted;
            GamemodeManager.OnGamemodeStopped -= OnGamemodeStopped;
        }

        [HideFromIl2Cpp]
        private void OnGamemodeStarted()
        {
            onGamemodeStartedHolder.Get()?.Invoke();
        }

        [HideFromIl2Cpp]
        private void OnGamemodeStopped()
        {
            onGamemodeStoppedHolder.Get()?.Invoke();
        }
#else
        public UltEventHolder onGamemodeStartedHolder;

        public UltEventHolder onGamemodeStoppedHolder;
#endif
    }
}