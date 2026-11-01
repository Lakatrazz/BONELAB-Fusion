using LabFusion.Network.Serialization;

namespace LabFusion.Entities;

/// <summary>
/// A serializable reference to a NetworkEntity.
/// <para>Currently, this just wraps NetworkEntityID. However, if you know that you want to send a reference to a network entity that actively exists over the network, it is recommended to use this instead.</para>
/// </summary>
public struct NetworkEntityReference : INetSerializable
{
    /// <summary>
    /// The size of a network entity reference in bytes.
    /// </summary>
    public static readonly int Size = NetworkEntityID.Size;

    /// <summary>
    /// The internal ID of the network entity.
    /// </summary>
    public NetworkEntityID ID;

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

    public NetworkEntityReference() : this(NetworkEntityID.Empty) { }

    public NetworkEntityReference(NetworkEntity entity) : this(entity.ID) { }

    public NetworkEntityReference(NetworkEntityID id)
    {
        ID = id;
    }
}
