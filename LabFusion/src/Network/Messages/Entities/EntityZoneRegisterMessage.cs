using System.Collections;

using Il2CppSLZ.Marrow.Interaction;

using LabFusion.Entities;
using LabFusion.Marrow.Zones;
using LabFusion.Network.Serialization;

using MelonLoader;

namespace LabFusion.Network;

public class EntityZoneRegisterMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.EntityZoneRegister;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<NetworkEntityReference>();

        if (!data.TryGetEntity(out var entity))
        {
            return;
        }

        if (received.Sender != entity.OwnerID.SmallID)
        {
            return;
        }

        var marrowExtender = entity.GetExtender<IMarrowEntityExtender>();

        if (marrowExtender == null)
        {
            return;
        }

        if (marrowExtender.MarrowEntity == null)
        {
            return;
        }

        // Unculling will cause the NetworkEntity to teleport to its pose
        // Sometimes it can get immediately culled right after, so do it for a few frames
        // Since its actually moving locations, this should make the zones track the entity properly
        MelonCoroutines.Start(RepeatCull(marrowExtender.MarrowEntity));
    }

    private static IEnumerator RepeatCull(MarrowEntity entity)
    {
        for (var i = 0; i < 3; i++)
        {
            SafeZoneCuller.Cull(entity, false);

            yield return null;
        }
    }
}