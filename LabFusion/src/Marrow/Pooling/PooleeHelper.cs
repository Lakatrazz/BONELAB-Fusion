using Il2CppSLZ.Marrow.Pool;

using LabFusion.Utilities;

using MelonLoader;

using System.Collections;

namespace LabFusion.Marrow;

public static class PooleeHelper
{
    public static void DespawnDelayed(Poolee poolee, float seconds)
    {
        MelonCoroutines.Start(CoDespawnDelayed(poolee, seconds));
    }

    private static IEnumerator CoDespawnDelayed(Poolee poolee, float seconds)
    {
        float elapsed = 0f;
        while (elapsed < seconds)
        {
            elapsed += TimeUtilities.DeltaTime;
            yield return null;
        }

        poolee.Despawn();
    }
}
