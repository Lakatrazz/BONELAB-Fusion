using System.Collections;

using LabFusion.Utilities;
using LabFusion.Entities;

using Il2CppSLZ.Marrow.Circuits;

using MelonLoader;

namespace LabFusion.Marrow.Extenders;

public class CircuitSocketExtender : EntityComponentExtender<CircuitSocket>
{
    public static readonly FusionComponentCache<CircuitSocket, NetworkEntity> Cache = new();

    public static readonly FusionComponentCache<ExternalCircuit, CircuitSocket> ExternalCache = new();

    private bool _registered = false;

    protected override void OnRegister(NetworkEntity networkEntity, CircuitSocket component)
    {
        Cache.Add(component, networkEntity);

        _registered = true;

        MelonCoroutines.Start(FindExternalCircuit());
    }

    protected override void OnUnregister(NetworkEntity networkEntity, CircuitSocket component)
    {
        Cache.Remove(component);

        _registered = false;

        OnExternalCircuitRemoved();
    }

    private IEnumerator FindExternalCircuit()
    {
        while (_registered && Component && !Component.externalCircuit)
        {
            yield return null;
        }

        if (!_registered)
        {
            yield break;
        }

        if (Component != null && Component.externalCircuit)
        {
            OnExternalCircuitFound(Component.externalCircuit);
        }
    }

    private void OnExternalCircuitFound(ExternalCircuit externalCircuit)
    {
        ExternalCache.Add(externalCircuit, Component);
    }

    private void OnExternalCircuitRemoved()
    {
        if (Component.externalCircuit == null)
        {
            return;
        }

        ExternalCache.Remove(Component.externalCircuit);
    }
}