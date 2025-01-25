using LabFusion.Patching;
using LabFusion.Entities;

namespace LabFusion.Network;

[Net.DelayWhileTargetLoading]
public class PuppetMasterKillMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.PuppetMasterKill;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<PropReferenceData>();

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