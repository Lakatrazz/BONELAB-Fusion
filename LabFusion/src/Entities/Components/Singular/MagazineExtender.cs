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

    private TimedDespawnHandler _despawnHandler = null;

    protected override void OnRegister(NetworkEntity entity, Magazine component)
    {
        Cache.Add(component, entity);

        if (NetworkInfo.IsHost)
        {
            _despawnHandler = new();
            _despawnHandler.Register(component.interactableHost, component._poolee);
        }

        entity.OnEntityDataCatchup += OnEntityDataCatchup;
    }

    protected override void OnUnregister(NetworkEntity entity, Magazine component)
    {
        Cache.Remove(component);

        if (_despawnHandler != null)
        {
            _despawnHandler.Unregister();
            _despawnHandler = null;
        }

        entity.OnEntityDataCatchup -= OnEntityDataCatchup;
    }

    private void OnEntityDataCatchup(NetworkEntity entity, PlayerID player)
    {
        // Send claim message
        var data = new MagazineClaimData() { OwnerID = PlayerIDManager.LocalSmallID, EntityID = entity.ID, Handedness = Handedness.UNDEFINED };

        MessageRelay.RelayModule<MagazineClaimMessage, MagazineClaimData>(data, new MessageRoute(player.SmallID, NetworkChannel.Reliable));
    }
}