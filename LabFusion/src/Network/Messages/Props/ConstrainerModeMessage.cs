using LabFusion.Entities;

using Il2CppSLZ.Marrow;

using LabFusion.Network.Serialization;

namespace LabFusion.Network;

public class ConstrainerModeData : INetSerializable
{
    public const int Size = sizeof(byte) * 2 + sizeof(ushort);

    public byte smallId;
    public ushort constrainerId;
    public Constrainer.ConstraintMode mode;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref smallId);
        serializer.SerializeValue(ref constrainerId);
        serializer.SerializeValue(ref mode, Precision.OneByte);
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
public class ConstrainerModeMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.ConstrainerMode;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<ConstrainerModeData>();

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