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

    public NetworkEntityReference() : this(NetworkEntityID.Empty) { }
    public NetworkEntityReference(NetworkEntity entity) : this(entity.ID) { }
    public NetworkEntityReference(NetworkEntityID id) { ID = id; }

    public readonly int? GetSize() => Size;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref ID);
    }

    /// <summary>
    /// Hooks a callback to run when the NetworkEntity is registered.
    /// <para>It is recommended to avoid this unless absolutely necessary, as the callback can potentially invoke for entities that reuse IDs.</para>
    /// </summary>
    /// <param name="callback"></param>
    public readonly void HookEntityRegistered(NetworkEntityDelegate callback) => NetworkEntityManager.HookEntityRegistered(ID, callback);

    /// <summary>
    /// Tries to get the referenced NetworkEntity and returns if it was found.
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public readonly bool TryGetEntity(out NetworkEntity entity) => NetworkEntityManager.TryGetEntity(ID, out entity);

    /// <summary>
    /// Gets and returns the NetworkEntity, or null if none is found.
    /// </summary>
    /// <returns></returns>
    public readonly NetworkEntity GetEntity() => NetworkEntityManager.GetEntity(ID);
}
