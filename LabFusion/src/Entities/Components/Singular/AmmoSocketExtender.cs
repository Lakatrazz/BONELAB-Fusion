using LabFusion.Utilities;
using LabFusion.Player;
using LabFusion.Network;
using LabFusion.Marrow.Messages;

using Il2CppSLZ.Marrow;

namespace LabFusion.Entities;

public class AmmoSocketExtender : EntityComponentExtender<AmmoSocket>
{
    public static readonly FusionComponentCache<AmmoSocket, NetworkEntity> Cache = new();

    protected override void OnRegister(NetworkEntity entity, AmmoSocket component)
    {
        Cache.Add(component, entity);

        entity.OnEntityDataCatchup += OnEntityDataCatchup;
    }

    protected override void OnUnregister(NetworkEntity entity, AmmoSocket component)
    {
        Cache.Remove(component);

        entity.OnEntityDataCatchup -= OnEntityDataCatchup;
    }

    private void OnEntityDataCatchup(NetworkEntity entity, PlayerID player)
    {
        if (Component.LockedPlug == null)
        {
            return;
        }

        if (!Component.gun)
        {
            return;
        }

        var gunEntity = GunExtender.Cache.Get(Component.gun);

        if (gunEntity == null)
        {
            return;
        }

        var ammoPlug = Component.LockedPlug.TryCast<AmmoPlug>();

        if (ammoPlug == null || ammoPlug.magazine == null)
        {
            return;
        }

        var magEntity = MagazineExtender.Cache.Get(ammoPlug.magazine);

        if (magEntity == null)
        {
            return;
        }

        var data = new MagazineInsertData() { MagazineId = magEntity.ID, GunId = gunEntity.ID };

        MessageRelay.RelayModule<MagazineInsertMessage, MagazineInsertData>(data, NetworkChannel.Reliable, RelayType.ToTarget, player.SmallID);
    }
}