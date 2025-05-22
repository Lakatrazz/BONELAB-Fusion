using LabFusion.Patching;
using LabFusion.Entities;
using LabFusion.Utilities;

namespace LabFusion.Network;

[Net.SkipHandleWhileLoading]
public class PuppetMasterKillMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.PuppetMasterKill;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<NetworkEntityReference>();

        data.HookEntityRegistered((entity) =>
        {
            var extender = entity.GetExtender<PuppetMasterExtender>();

            if (extender == null)
            {
                return;
            }

            // Save the most recent killed NPC
            PuppetMasterExtender.LastKilled = entity;

            // Kill the puppet
            PuppetMasterPatches.IgnorePatches = true;

            try
            {
                extender.Component.Kill();
            }
            catch (Exception e)
            {
                FusionLogger.LogException("executing PuppetMaster.Kill", e);
            }

            PuppetMasterPatches.IgnorePatches = false;
        });
    }
}