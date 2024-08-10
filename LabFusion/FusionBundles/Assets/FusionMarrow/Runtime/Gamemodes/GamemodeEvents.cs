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

        public Il2CppReferenceField<UltEvent> onGamemodeStarted;

        public Il2CppReferenceField<UltEvent> onGamemodeEnded;

        private UltEvent _onGamemodeStartedCached = null;
        private UltEvent _onGamemodeEndedCached = null;

        private void Awake()
        {
            _onGamemodeStartedCached = onGamemodeStarted.Get();
            _onGamemodeEndedCached = onGamemodeEnded.Get();

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
                _onGamemodeStartedCached?.Invoke();
            }
            else 
            {
                _onGamemodeEndedCached?.Invoke();
            }
        }
#else
        public UltEvent onGamemodeStarted;

        public UltEvent onGamemodeEnded;
#endif
    }
}