namespace LabFusion.Entities;

public interface INetworkRegistrable
{
    NetworkEntityID ID { get; }

    bool IsRegistered { get; }

    bool IsDestroyed { get; }

    void Register(NetworkEntityID id);

    void Unregister();
}