using Il2CppSLZ.Marrow.Interaction;

using LabFusion.Utilities;

namespace LabFusion.Entities;

public interface IMarrowEntityExtender : IEntityExtender
{
    public static readonly FusionComponentCache<MarrowEntity, NetworkEntity> Cache = new();

    NetworkEntity NetworkEntity { get; }

    MarrowEntity MarrowEntity { get; }

    event Action OnBeforeTeleportToPose;
    event Action OnAfterTeleportToPose;

    void OnEntityCull(bool isInactive);

    void TeleportToPose();

    void TeleportToPoseWithoutNotify();

    void HookOnReady(Action callback);
}
