namespace LabFusion.Entities;

public interface INetworkRegistrable
{
    ushort ID { get; }

    bool IsRegistered { get; }

    bool IsDestroyed { get; }

    void Register(ushort id);

    void Unregister();
}