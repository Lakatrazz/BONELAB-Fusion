using LabFusion.Network.Serialization;

namespace LabFusion.Entities;

/// <summary>
/// A serializable reference to a NetworkEntity.
/// </summary>
public struct NetworkEntityReference : INetSerializable
{
    public const int Size = sizeof(ushort);

    public ushort ID;

    public readonly int? GetSize() => Size;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref ID);
    }

    public readonly void HookEntityRegistered(NetworkEntityDelegate callback)
    {
        NetworkEntityManager.HookEntityRegistered(ID, callback);
    }

    public readonly bool TryGetEntity(out NetworkEntity entity)
    {
        entity = GetEntity();

        return entity != null;
    }

    public readonly NetworkEntity GetEntity()
    {
        return NetworkEntityManager.IDManager.RegisteredEntities.GetEntity(ID);
    }

    public NetworkEntityReference() : this(0) { }

    public NetworkEntityReference(NetworkEntity entity) : this(entity.ID) { }

    public NetworkEntityReference(ushort id)
    {
        ID = id;
    }
}
