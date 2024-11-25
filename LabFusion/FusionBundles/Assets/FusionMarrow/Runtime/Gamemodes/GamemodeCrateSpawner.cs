using UnityEngine;

#if MELONLOADER
using MelonLoader;

using Il2CppInterop.Runtime.Attributes;

using Il2CppSLZ.Marrow.Warehouse;
using Il2CppSLZ.Marrow.Pool;

using LabFusion.SDK.Gamemodes;
#else
using SLZ.Marrow.Warehouse;
#endif

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace LabFusion.Marrow.Integration
{
#if MELONLOADER
    [RegisterTypeInIl2Cpp]
#else
    [RequireComponent(typeof(CrateSpawner))]
    [DisallowMultipleComponent]
    [HelpURL("https://github.com/Lakatrazz/BONELAB-Fusion/wiki/Gamemode-Maps#gamemode-crate-spawners")]
#endif
    public class GamemodeCrateSpawner : MonoBehaviour
    {
#if MELONLOADER
        public GamemodeCrateSpawner(IntPtr intPtr) : base(intPtr) { }

        private CrateSpawner _crateSpawner = null;
        private Poolee _spawnedPoolee = null;

        private void Awake()
        {
            _crateSpawner = GetComponent<CrateSpawner>();
            _crateSpawner.manualMode = true;
            _crateSpawner.onSpawnEvent.add_DynamicCalls((Il2CppSystem.Action<CrateSpawner, GameObject>)OnSpawnEvent);

            GamemodeManager.OnGamemodeStarted += OnGamemodeStarted;
            GamemodeManager.OnGamemodeStopped += OnGamemodeStopped;
        }

        private void OnDestroy()
        {
            GamemodeManager.OnGamemodeStarted -= OnGamemodeStarted;
            GamemodeManager.OnGamemodeStopped -= OnGamemodeStopped;
        }

        private void OnSpawnEvent(CrateSpawner spawner, GameObject spawned)
        {
            _spawnedPoolee = spawned.GetComponent<Poolee>();
        }

        [HideFromIl2Cpp]
        private void OnGamemodeStarted()
        {
            Spawn();
        }

        [HideFromIl2Cpp]
        private void OnGamemodeStopped()
        {
            Despawn();
        }

        private void Spawn()
        {
            if (_spawnedPoolee != null)
            {
                Despawn();
            }

            _crateSpawner.SpawnSpawnable();
        }

        private void Despawn()
        {
            if (_spawnedPoolee == null)
            {
                return;
            }

            _spawnedPoolee.Despawn();
            _spawnedPoolee = null;
        }
#else
        private void Reset()
        {
            if (!TryGetComponent<CrateSpawner>(out var crateSpawner))
            {
                return;
            }

            crateSpawner.manualMode = true;

#if UNITY_EDITOR
            EditorUtility.SetDirty(crateSpawner);
#endif
        }
#endif
    }
}