namespace LabFusion.Entities;

/// <summary>
/// Identifies the source of a NetworkEntity's creation.
/// </summary>
public enum EntitySource
{
    /// <summary>
    /// No source has been given.
    /// </summary>
    None,

    /// <summary>
    /// The entity was part of the scene or was spawned in for the scene.
    /// </summary>
    Scene,

    /// <summary>
    /// The entity was spawned in or created by a player.
    /// </summary>
    Player
}
