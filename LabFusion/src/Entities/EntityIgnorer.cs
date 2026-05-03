using Il2CppSLZ.Marrow.Interaction;

using LabFusion.Extensions;
using LabFusion.Utilities;

namespace LabFusion.Entities;

/// <summary>
/// Tracks MarrowEntities that collision can be ignored with for a specified amount of time.
/// </summary>
public sealed class EntityIgnorer
{
    /// <summary>
    /// The MarrowEntity that will ignore other entities.
    /// </summary>
    public MarrowEntity MarrowEntity { get; }

    /// <summary>
    /// The current ignored entities and the time remaining until they are no longer ignored.
    /// </summary>
    public Dictionary<MarrowEntity, float> IgnoringEntities { get; } = new(new UnityComparer());

    public EntityIgnorer(MarrowEntity marrowEntity)
    {
        MarrowEntity = marrowEntity;
    }

    /// <summary>
    /// Ignores collision with a MarrowEntity for a specified length.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="duration"></param>
    public void TimedIgnoreEntity(MarrowEntity entity, float duration)
    {
        IgnoringEntities[entity] = duration;

        IgnoreCollision(entity, true);
    }

    /// <summary>
    /// Stops ignoring collision with a MarrowEntity.
    /// </summary>
    /// <param name="entity"></param>
    public void CancelIgnoreEntity(MarrowEntity entity)
    {
        IgnoringEntities.Remove(entity);

        IgnoreCollision(entity, false);
    }

    private void IgnoreCollision(MarrowEntity otherEntity, bool ignore)
    {
        if (otherEntity == null)
        {
            return;
        }

        try
        {
            MarrowEntity.IgnoreCollision(otherEntity, ignore);
        }
        catch (Exception e)
        {
            FusionLogger.LogException("ignoring entity", e);
        }
    }

    public void Tick(float deltaTime)
    {
        if (IgnoringEntities.Count <= 0)
        {
            return;
        }

        bool hasInvalidEntity = false;

        foreach (var pair in IgnoringEntities)
        {
            var entity = pair.Key;
            var time = pair.Value;

            time -= deltaTime;

            if (entity == null)
            {
                time = 0f;
            }

            if (time <= 0f)
            {
                hasInvalidEntity = true;
            }

            IgnoringEntities[entity] = time;
        }

        if (hasInvalidEntity)
        {
            var invalidEntities = IgnoringEntities.Where(p => p.Value <= 0f);

            foreach (var entity in invalidEntities)
            {
                CancelIgnoreEntity(entity.Key);
            }
        }
    }
}
