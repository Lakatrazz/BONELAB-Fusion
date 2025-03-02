using Il2CppSLZ.Marrow.Interaction;

using LabFusion.Utilities;

namespace LabFusion.Entities;

public interface IMarrowEntityExtender : IEntityExtender
{
    public static readonly FusionComponentCache<MarrowEntity, NetworkEntity> Cache = new();

    NetworkEntity NetworkEntity { get; }

    MarrowEntity MarrowEntity { get; }

    void OnEntityCull(bool isInactive);

    void TeleportToPose();

    void HookOnReady(Action callback);
}
