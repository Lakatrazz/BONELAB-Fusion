using LabFusion.Patching;
using LabFusion.Entities;

namespace LabFusion.Network;

[Net.DelayWhileTargetLoading]
public class PuppetMasterKillMessage : FusionMessageHandler
{
    public override byte? Tag => NativeMessageTag.PuppetMasterKill;

    public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
    {
        using FusionReader reader = FusionReader.Create(bytes);
        var data = reader.ReadFusionSerializable<PropReferenceData>();

        // Send message to other clients if server
        if (isServerHandled)
        {
            using var message = FusionMessage.Create(Tag.Value, bytes);
            MessageSender.BroadcastMessageExcept(data.smallId, NetworkChannel.Reliable, message, false);

            return;
        }

        var entity = NetworkEntityManager.IdManager.RegisteredEntities.GetEntity(data.syncId);

        if (entity == null)
        {
            return;
        }

        var extender = entity.GetExtender<PuppetMasterExtender>();

        if (extender == null)
        {
            return;
        }

        // Save the most recent killed NPC
        PuppetMasterExtender.LastKilled = entity;

        // Kill the puppet
        PuppetMasterPatches.IgnorePatches = true;
        extender.Component.Kill();
        PuppetMasterPatches.IgnorePatches = false;
    }
}