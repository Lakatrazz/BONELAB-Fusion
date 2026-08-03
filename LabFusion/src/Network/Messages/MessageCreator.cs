using LabFusion.Network.Serialization;
using LabFusion.SDK.Modules;

namespace LabFusion.Network.Messages;

public static class MessageCreator
{
    public static NetMessage CreateNative<TData>(TData data, byte tag, MessageRoute route) where TData : INetSerializable
    {
        using var writer = NetWriter.Create(data.GetSize());

        data.Serialize(writer);

        return NetMessage.Create(tag, writer, route);
    }

    public static NetMessage CreateModule<TMessage, TData>(TData data, MessageRoute route) where TMessage : ModuleMessageHandler where TData : INetSerializable
    {
        using var writer = NetWriter.Create(data.GetSize());

        data.Serialize(writer);

        return NetMessage.ModuleCreate<TMessage>(writer, route);
    }
}
