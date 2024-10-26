using LabFusion.Utilities;
using LabFusion.Entities;

using Il2CppSLZ.Marrow.Circuits;

namespace LabFusion.Marrow.Extenders;

public class ButtonControllerExtender : EntityComponentExtender<ButtonController>
{
    public static readonly FusionComponentCache<ButtonController, NetworkEntity> Cache = new();

    public bool Charged { get; set; } = false;

    protected override void OnRegister(NetworkEntity networkEntity, ButtonController component)
    {
        Cache.Add(component, networkEntity);
    }

    protected override void OnUnregister(NetworkEntity networkEntity, ButtonController component)
    {
        Cache.Remove(component);
    }
}