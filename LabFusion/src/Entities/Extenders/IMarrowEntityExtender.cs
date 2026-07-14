using Il2CppSLZ.Marrow.Interaction;

using LabFusion.Utilities;

namespace LabFusion.Entities;

/// <summary>
/// Extends behavior on a NetworkEntity to support a physical MarrowEntity.
/// </summary>
public interface IMarrowEntityExtender : IEntityExtender
{
    /// <summary>
    /// The global cache for looking up a NetworkEntity based on a MarrowEntity component.
    /// </summary>
    public static readonly FusionComponentCache<MarrowEntity, NetworkEntity> Cache = new();
    
    /// <summary>
    /// The NetworkEntity attached to this extender.
    /// </summary>
    NetworkEntity NetworkEntity { get; }

    /// <summary>
    /// The MarrowEntity attached to this extender.
    /// </summary>
    MarrowEntity MarrowEntity { get; }

    /// <summary>
    /// Invoked before the entity teleports to its target pose.
    /// </summary>
    event Action OnBeforeTeleportToPose;

    /// <summary>
    /// Invoked after the entity teleports to its target pose.
    /// </summary>
    event Action OnAfterTeleportToPose;

    /// <summary>
    /// Informs the extender about the MarrowEntity's cull state. This should be invoked after the MarrowEntity is culled.
    /// </summary>
    /// <param name="isInactive"></param>
    void OnEntityCull(bool isInactive);

    /// <summary>
    /// Teleports the entity to its target pose.
    /// </summary>
    void TeleportToPose();

    /// <summary>
    /// Teleports the entity to its target pose, without running the OnBeforeTeleportToPose and OnAfterTeleportToPose callbacks.
    /// This should be used if the entity needs to be teleported within one of these callbacks to prevent a potential infinite cycle.
    /// </summary>
    void TeleportToPoseWithoutNotify();

    /// <summary>
    /// Hooks a callback to run whenever the MarrowEntity exists, or runs it instantly if it already does.
    /// </summary>
    /// <param name="callback"></param>
    void HookOnReady(Action callback);
}
