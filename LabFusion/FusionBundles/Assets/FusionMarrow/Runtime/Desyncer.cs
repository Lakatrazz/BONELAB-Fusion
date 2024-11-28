#if MELONLOADER
using LabFusion.Utilities;

using MelonLoader;
#endif

using UnityEngine;

namespace LabFusion.Marrow.Integration
{
#if MELONLOADER
    [RegisterTypeInIl2Cpp]
#else
    [DisallowMultipleComponent]
    [HelpURL("https://github.com/Lakatrazz/BONELAB-Fusion/wiki/Modifier-Scripts#desyncer")]
#endif
    public class Desyncer : MonoBehaviour
    {
#if MELONLOADER
        public static FusionComponentCache<GameObject, Desyncer> Cache { get; } = new();

        private void Awake()
        {
            Cache.Add(gameObject, this);
        }

        private void OnDestroy()
        {
            Cache.Remove(gameObject);
        }
#endif
    }
}