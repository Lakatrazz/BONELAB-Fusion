using Il2CppSLZ.Marrow.Circuits;

using LabFusion.Entities;
using LabFusion.Marrow.Extenders;

namespace LabFusion.Marrow.Circuits;

public static class CircuitHelper
{
    public static NetworkEntity GetNetworkEntity(Circuit circuit)
    {
        if (circuit == null)
        {
            return null;
        }

        var externalCircuit = circuit.TryCast<ExternalCircuit>();

        if (externalCircuit != null && CircuitSocketExtender.ExternalCache.TryGet(externalCircuit, out var internalCircuit))
        {
            return CircuitSocketExtender.Cache.Get(internalCircuit);
        }

        return null;
    }
}
