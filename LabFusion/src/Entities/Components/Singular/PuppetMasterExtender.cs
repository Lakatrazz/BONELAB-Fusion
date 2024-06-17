using LabFusion.Utilities;

using Il2CppSLZ.Marrow.PuppetMasta;

namespace LabFusion.Entities;

public class PuppetMasterExtender : EntityComponentExtender<PuppetMaster>
{
    public static FusionComponentCache<PuppetMaster, NetworkEntity> Cache = new();

    public static NetworkEntity LastKilled = null;

    protected override void OnRegister(NetworkEntity networkEntity, PuppetMaster component)
    {
        Cache.Add(component, networkEntity);
    }

    protected override void OnUnregister(NetworkEntity networkEntity, PuppetMaster component)
    {
        Cache.Remove(component);
    }
}