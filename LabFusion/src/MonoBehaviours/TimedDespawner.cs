using Il2CppInterop.Runtime.Attributes;

using Il2CppSLZ.Marrow.Pool;

using LabFusion.Utilities;

using MelonLoader;

using UnityEngine;

namespace LabFusion.MonoBehaviours;

[RegisterTypeInIl2Cpp]
public class TimedDespawner : MonoBehaviour
{
    public delegate bool DespawnCheck();

    public TimedDespawner(IntPtr intPtr) : base(intPtr) { }

    // Cleanable objects should usually despawn pretty quickly
    public const float TotalDespawnTime = 10f;

    private Poolee _poolee = null;
    private float _elapsedTime = 0f;

    public Poolee Poolee { get { return _poolee; } set { _poolee = value; } }

    [HideFromIl2Cpp]
    public event DespawnCheck OnDespawnCheck;

    [HideFromIl2Cpp]
    public void RefreshTimer()
    {
        _elapsedTime = 0f;
    }

    private void OnEnable()
    {
        RefreshTimer();
    }

    private void OnDisable()
    {
        RefreshTimer();
    }

    private void LateUpdate()
    {
        _elapsedTime += TimeUtilities.DeltaTime;

        // Despawn the poolee
        if (_elapsedTime >= TotalDespawnTime)
        {
            // Make sure to check that the despawn is valid, incase theres an event preventing the despawn
            var despawnValid = OnDespawnCheck?.Invoke();

            if (despawnValid.HasValue && !despawnValid.Value)
            {
                RefreshTimer();
                return;
            }

            _poolee.Despawn();
        }
    }
}