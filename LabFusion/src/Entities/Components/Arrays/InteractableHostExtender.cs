using Il2CppSLZ.Interaction;

using LabFusion.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Entities;

public class InteractableHostExtender : EntityComponentArrayExtender<InteractableHost>
{
    public static readonly FusionComponentCache<InteractableHost, NetworkEntity> Cache = new();

    protected override void OnRegister(NetworkEntity networkEntity, InteractableHost[] components)
    {
        foreach (var host in components)
        {
            Cache.Add(host, networkEntity);
        }
    }

    protected override void OnUnregister(NetworkEntity networkEntity, InteractableHost[] components)
    {
        foreach (var host in components)
        {
            Cache.Remove(host);
        }
    }
}