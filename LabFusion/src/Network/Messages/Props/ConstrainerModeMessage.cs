using LabFusion.Data;
using LabFusion.Entities;

using Il2CppSLZ.Bonelab;

namespace LabFusion.Network;

public class ConstrainerModeData : IFusionSerializable
{
    public const int Size = sizeof(byte) * 2 + sizeof(ushort);

    public byte smallId;
    public ushort constrainerId;
    public Constrainer.ConstraintMode mode;

    public void Serialize(FusionWriter writer)
    {
        writer.Write(smallId);
        writer.Write(constrainerId);
        writer.Write((byte)mode);
    }

    public void Deserialize(FusionReader reader)
    {
        smallId = reader.ReadByte();
        constrainerId = reader.ReadUInt16();
        mode = (Constrainer.ConstraintMode)reader.ReadByte();
    }

    public static ConstrainerModeData Create(byte smallId, ushort constrainerId, Constrainer.ConstraintMode mode)
    {
        return new ConstrainerModeData()
        {
            smallId = smallId,
            constrainerId = constrainerId,
            mode = mode,
        };
    }
}

[Net.SkipHandleWhileLoading]
public class ConstrainerModeMessage : FusionMessageHandler
{
    public override byte? Tag => NativeMessageTag.ConstrainerMode;

    public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
    {
        using FusionReader reader = FusionReader.Create(bytes);
        var data = reader.ReadFusionSerializable<ConstrainerModeData>();

        // Send message to all clients if server
        if (isServerHandled)
        {
            using var message = FusionMessage.Create(Tag.Value, bytes);
            MessageSender.BroadcastMessageExcept(data.smallId, NetworkChannel.Reliable, message, false);

            return;
        }

        var constrainer = NetworkEntityManager.IdManager.RegisteredEntities.GetEntity(data.constrainerId);

        if (constrainer == null)
        {
            return;
        }

        var extender = constrainer.GetExtender<ConstrainerExtender>();

        if (extender == null)
        {
            return;
        }

        // Change the mode
        var comp = extender.Component;

        comp.mode = data.mode;
    }
}