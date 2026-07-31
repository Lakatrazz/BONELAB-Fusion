namespace LabFusion.Entities;

/// <summary>
/// Implements behavior regarding NetworkEntities being despawnable.
/// </summary>
public interface IEntityDespawnableExtender : IEntityExtender
{
    /// <summary>
    /// Despawns the NetworkEntity locally. This will not despawn it for all clients in the server.
    /// </summary>
    void OnDespawnReceived();

    /// <summary>
    /// Plays any visual effects that are part of a despawn.
    /// </summary>
    void PlayDespawnEffect();
}
