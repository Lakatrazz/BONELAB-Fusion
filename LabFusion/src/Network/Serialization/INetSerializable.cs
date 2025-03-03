namespace LabFusion.Network.Serialization;

public interface INetSerializable
{
    /// <summary>
    /// Returns the size, in bytes, of the INetSerializable. If returning null, the default capacity will be used.
    /// It is recommended to implement this, but you need to make sure you return a size equal to or larger than the amount of bytes being written.
    /// </summary>
    /// <returns></returns>
    int? GetSize() => null;

    /// <summary>
    /// Writes or reads information from the INetSerializable using the INetSerializer.
    /// </summary>
    /// <param name="serializer"></param>
    void Serialize(INetSerializer serializer);
}
