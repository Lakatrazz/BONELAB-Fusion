using Il2CppInterop.Runtime.InteropTypes.Arrays;

using UnityEngine;

using MarrowLayers = Il2CppSLZ.Marrow.Interaction.MarrowLayers;

namespace LabFusion.Audio;

public static class AudioOcclusion
{
    public static readonly int OcclusionMask = ~0
        & ~(1 << (int)MarrowLayers.Player)
        & ~(1 << (int)MarrowLayers.EnemyColliders)
        & ~(1 << (int)MarrowLayers.Football)
        & ~(1 << (int)MarrowLayers.FootballOnly)
        & ~(1 << (int)MarrowLayers.ObserverTracker)
        & ~(1 << (int)MarrowLayers.ObserverTrigger)
        & ~(1 << (int)MarrowLayers.EntityTracker)
        & ~(1 << (int)MarrowLayers.EntityTrigger)
        & ~(1 << (int)MarrowLayers.BeingTracker)
        & ~(1 << (int)MarrowLayers.BeingTrigger)
        & ~(1 << (int)MarrowLayers.Deciverse);

    public const int MaxIteration = 8;

    private static readonly Il2CppStructArray<RaycastHit> _hitBuffer = new(32);

    public static float RaycastOcclusionMultiplier(Vector3 listener, Vector3 source)
    {
        float thickness = 0f;

        if (RaycastWallThickness(listener, source, ref thickness))
        {
            return MathF.Exp(-MathF.Sqrt(thickness * 3f));
        }

        return 1f;
    }

    private static bool RaycastWallThickness(Vector3 start, Vector3 end, ref float thickness, int iteration = 0)
    {
        if (iteration >= MaxIteration)
        {
            return false;
        }

        bool endToStart = Physics.Linecast(end, start, out var endToStartInfo, OcclusionMask, QueryTriggerInteraction.Ignore);

        bool startToEnd = false;
        RaycastHit startToEndInfo = default;

        if (endToStart)
        {
            startToEnd = RaycastClosestToEnd(start, endToStartInfo.point, endToStartInfo.rigidbody, out startToEndInfo);
        }

        if (startToEnd && endToStart)
        {
            thickness += (startToEndInfo.point - endToStartInfo.point).magnitude;

            RaycastWallThickness(start, startToEndInfo.point, ref thickness, iteration + 1);

            return true;
        }

        return false;
    }

    private static bool RaycastClosestToEnd(Vector3 start, Vector3 end, Rigidbody rigidbody, out RaycastHit hitInfo)
    {
        bool raycast = false;
        hitInfo = default;

        float closestDistance = float.PositiveInfinity;

        var direction = end - start;

        var count = Physics.RaycastNonAlloc(new Ray(start, direction.normalized), _hitBuffer, direction.magnitude, OcclusionMask, QueryTriggerInteraction.Ignore);

        for (var i = 0; i < count; i++)
        {
            var hit = _hitBuffer[i];

            if (hit.rigidbody != rigidbody)
            {
                continue;
            }

            var distance = (hit.point - end).sqrMagnitude;

            if (distance < closestDistance)
            {
                closestDistance = distance;
                raycast = true;
                hitInfo = hit;
            }
        }

        return raycast;
    }
}
