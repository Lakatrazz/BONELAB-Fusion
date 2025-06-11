using LabFusion.Entities;
using LabFusion.Network;
using LabFusion.Network.Serialization;
using LabFusion.SDK.Modules;

namespace LabFusion.Marrow.Messages;

public class InventoryAmmoReceiverDropData : INetSerializable
{
    public const int Size = sizeof(ushort);

    public int? GetSize() => Size;

    public ushort EntityID;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref EntityID);
    }
}

[Net.SkipHandleWhileLoading]
public class InventoryAmmoReceiverDropMessage : ModuleMessageHandler
{
    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<InventoryAmmoReceiverDropData>();

        var entity = NetworkEntityManager.IDManager.RegisteredEntities.GetEntity(data.EntityID);

        if (entity == null)
        {
            return;
        }

        var ammoReceiverExtender = entity.GetExtender<InventoryAmmoReceiverExtender>();

        if (ammoReceiverExtender == null)
        {
            return;
        }

        var ammoReceiver = ammoReceiverExtender.Component;

        LocalAudioPlayer.PlayAtPoint(ammoReceiver.grabClips, ammoReceiver.transform.position, new AudioPlayerSettings()
        {
            Mixer = LocalAudioPlayer.SoftInteraction,
            Volume = 0.2f,
        });
    }
}