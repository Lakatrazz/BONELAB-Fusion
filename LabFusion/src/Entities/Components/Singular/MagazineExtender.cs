using LabFusion.Utilities;
using LabFusion.Network;
using LabFusion.Player;
using LabFusion.Marrow.Messages;

using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Interaction;

namespace LabFusion.Entities;

public class MagazineExtender : EntityComponentExtender<Magazine>
{
    public static readonly FusionComponentCache<Magazine, NetworkEntity> Cache = new();

    private EntityCleaner _cleaner = null;

    protected override void OnRegister(NetworkEntity entity, Magazine component)
    {
        Cache.Add(component, entity);

        RegisterCleaner();

        entity.OnEntityDataCatchup += OnEntityDataCatchup;
    }

    protected override void OnUnregister(NetworkEntity entity, Magazine component)
    {
        Cache.Remove(component);

        UnregisterCleaner();

        entity.OnEntityDataCatchup -= OnEntityDataCatchup;
    }

    private void RegisterCleaner()
    {
        _cleaner = new();
        _cleaner.Register(NetworkEntity, Component.interactableHost, Component._poolee);
    }

    private void UnregisterCleaner()
    {
        if (_cleaner != null)
        {
            _cleaner.Unregister();
            _cleaner = null;
        }
    }

    private void OnEntityDataCatchup(NetworkEntity entity, PlayerID player)
    {
        // Send claim message
        var data = new MagazineClaimData() { OwnerID = PlayerIDManager.LocalSmallID, EntityID = entity.ID, Handedness = Handedness.UNDEFINED };

        MessageRelay.RelayModule<MagazineClaimMessage, MagazineClaimData>(data, new MessageRoute(player.SmallID, NetworkChannel.Reliable));
    }
}