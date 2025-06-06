using LabFusion.Entities;
using LabFusion.SDK.Modules;
using LabFusion.Network;
using LabFusion.Network.Serialization;

using Il2CppSLZ.Marrow;

namespace LabFusion.Marrow.Messages;

public class ConstrainerModeData : INetSerializable
{
    public const int Size = sizeof(byte) + sizeof(ushort);

    public int? GetSize() => Size;

    public ushort ConstrainerID;
    public Constrainer.ConstraintMode Mode;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref ConstrainerID);
        serializer.SerializeValue(ref Mode, Precision.OneByte);
    }

    public static ConstrainerModeData Create(byte smallId, ushort constrainerId, Constrainer.ConstraintMode mode)
    {
        return new ConstrainerModeData()
        {
            ConstrainerID = constrainerId,
            Mode = mode,
        };
    }
}

[Net.SkipHandleWhileLoading]
public class ConstrainerModeMessage : ModuleMessageHandler
{
    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<ConstrainerModeData>();

        var constrainer = NetworkEntityManager.IDManager.RegisteredEntities.GetEntity(data.ConstrainerID);

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

        comp.mode = data.Mode;
    }
}