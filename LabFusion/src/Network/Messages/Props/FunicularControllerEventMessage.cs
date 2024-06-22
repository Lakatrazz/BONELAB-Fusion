using LabFusion.Data;
using LabFusion.Patching;
using LabFusion.Exceptions;
using LabFusion.Entities;

namespace LabFusion.Network;

public enum FunicularControllerEventType
{
    UNKNOWN = 0,
    CARTGO = 1,
    CARTFORWARDS = 2,
    CARTBACKWARDS = 3,
}

public class FunicularControllerEventData : IFusionSerializable
{
    public const int Size = sizeof(byte) + sizeof(ushort);

    public ushort syncId;
    public FunicularControllerEventType type;

    public void Serialize(FusionWriter writer)
    {
        writer.Write(syncId);
        writer.Write((byte)type);
    }

    public void Deserialize(FusionReader reader)
    {
        syncId = reader.ReadUInt16();
        type = (FunicularControllerEventType)reader.ReadByte();
    }

    public static FunicularControllerEventData Create(ushort syncId, FunicularControllerEventType type)
    {
        return new FunicularControllerEventData()
        {
            syncId = syncId,
            type = type,
        };
    }
}

[Net.DelayWhileTargetLoading]
public class FunicularControllerEventMessage : FusionMessageHandler
{
    public override byte? Tag => NativeMessageTag.FunicularControllerEvent;

    public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
    {
        using FusionReader reader = FusionReader.Create(bytes);
        var data = reader.ReadFusionSerializable<FunicularControllerEventData>();

        if (isServerHandled)
        {
            throw new ExpectedClientException();
        }

        var entity = NetworkEntityManager.IdManager.RegisteredEntities.GetEntity(data.syncId);

        if (entity == null)
        {
            return;
        }

        var extender = entity.GetExtender<FunicularControllerExtender>();

        if (extender == null)
        {
            return;
        }

        FunicularControllerPatches.IgnorePatches = true;

        switch (data.type)
        {
            default:
            case FunicularControllerEventType.UNKNOWN:
                break;
            case FunicularControllerEventType.CARTGO:
                extender.Component.CartGo();
                break;
            case FunicularControllerEventType.CARTFORWARDS:
                extender.Component.CartForwards();
                break;
            case FunicularControllerEventType.CARTBACKWARDS:
                extender.Component.CartBackwards();
                break;
        }

        FunicularControllerPatches.IgnorePatches = false;
    }
}