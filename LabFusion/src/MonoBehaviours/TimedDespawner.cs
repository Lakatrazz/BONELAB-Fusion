using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Il2CppInterop.Runtime.Attributes;

using Il2CppSLZ.Marrow.Pool;

using LabFusion.Network;
using LabFusion.Utilities;

using MelonLoader;

using UnityEngine;

namespace LabFusion.MonoBehaviours;

[RegisterTypeInIl2Cpp]
public class TimedDespawner : MonoBehaviour
{
    public TimedDespawner(IntPtr intPtr) : base(intPtr) { }

    public const float TotalDespawnTime = 30f;

    private Poolee _poolee = null;
    private float _elapsedTime = 0f;

    public Poolee Poolee { get { return _poolee; } set { _poolee = value; } }

    [HideFromIl2Cpp]
    public void RefreshTimer()
    {
        _elapsedTime = 0f;
    }

    private void LateUpdate()
    {
        // Make sure this is the server, otherwise we shouldn't despawn it
        if (!NetworkInfo.IsServer)
        {
            return;
        }

        _elapsedTime += TimeUtilities.DeltaTime;

        // Despawn the poolee
        if (_elapsedTime >= TotalDespawnTime)
        {
            _poolee.Despawn();
        }
    }
}