using LabFusion.Utilities;
using LabFusion.Player;
using LabFusion.Network;
using LabFusion.Marrow.Messages;
using LabFusion.Entities;

using Il2CppSLZ.Marrow;

namespace LabFusion.Marrow.Extenders;

public class AmmoSocketExtender : EntityComponentExtender<AmmoSocket>
{
    protected override void OnRegister(NetworkEntity entity, AmmoSocket component)
    {
        entity.OnEntityDataCatchup += OnEntityDataCatchup;
    }

    protected override void OnUnregister(NetworkEntity entity, AmmoSocket component)
    {
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

        magEntity.HookOnDataCatchup(player, (magEntity, magPlayer) =>
        {
            var data = new MagazineInsertData() { MagazineID = magEntity.ID, GunID = gunEntity.ID };

            MessageRelay.RelayModule<MagazineInsertMessage, MagazineInsertData>(data, new MessageRoute(player.SmallID, NetworkChannel.Reliable));
        });
    }
}