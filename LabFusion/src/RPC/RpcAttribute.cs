using LabFusion.Network;

namespace LabFusion.RPC;

[AttributeUsage(AttributeTargets.Method)]
public class RpcAttribute : Attribute
{
    public RelayType RelayType = RelayType.ToClients;

    public NetworkChannel Channel = NetworkChannel.Reliable;

    private long _methodHash = 0;
    public long MethodHash => _methodHash;

    public RpcAttribute() { }

    public RpcAttribute(RelayType relayType)
    {
        RelayType = relayType;
    }

    public void SetHash(long hash)
    {
        _methodHash = hash;
    }
}
