using LabFusion.Utilities;
using LabFusion.Player;
using LabFusion.Network;
using LabFusion.Entities;
using LabFusion.Marrow.Messages;

using Il2CppSLZ.Marrow.PuppetMasta;

namespace LabFusion.Marrow.Extenders;

public class PuppetMasterExtender : EntityComponentExtender<PuppetMaster>
{
    public static readonly FusionComponentCache<PuppetMaster, NetworkEntity> Cache = new();

    public static NetworkEntity LastKilled { get; set; } = null;

    protected override void OnRegister(NetworkEntity entity, PuppetMaster component)
    {
        Cache.Add(component, entity);

        entity.OnEntityDataCatchup += OnEntityDataCatchup;
    }

    protected override void OnUnregister(NetworkEntity entity, PuppetMaster component)
    {
        Cache.Remove(component);

        entity.OnEntityDataCatchup -= OnEntityDataCatchup;
    }

    private void OnEntityDataCatchup(NetworkEntity entity, PlayerID player)
    {
        if (Component.isDead)
        {
            var data = new NetworkEntityReference(entity);

            MessageRelay.RelayModule<PuppetMasterKillMessage, NetworkEntityReference>(data, new MessageRoute(player.SmallID, NetworkChannel.Reliable));
        }
    }
}