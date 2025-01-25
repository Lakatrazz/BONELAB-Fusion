using LabFusion.Data;
using LabFusion.Bonelab.Patching;
using LabFusion.Entities;
using LabFusion.Network;
using LabFusion.SDK.Modules;
using LabFusion.Bonelab.Extenders;

namespace LabFusion.Bonelab;

public class FlashlightToggleData : IFusionSerializable
{
    public const int Size = sizeof(byte) * 2 + sizeof(ushort);

    public ushort entityId;
    public bool isEnabled;

    public void Serialize(FusionWriter writer)
    {
        writer.Write(entityId);
        writer.Write(isEnabled);
    }

    public void Deserialize(FusionReader reader)
    {
        entityId = reader.ReadUInt16();
        isEnabled = reader.ReadBoolean();
    }

    public static FlashlightToggleData Create(ushort syncId, bool isEnabled)
    {
        return new FlashlightToggleData()
        {
            entityId = syncId,
            isEnabled = isEnabled,
        };
    }
}

[Net.DelayWhileTargetLoading]
public class FlashlightToggleMessage : ModuleMessageHandler
{
    protected override void OnHandleMessage(ReceivedMessage received)
    {
        using FusionReader reader = FusionReader.Create(received.Bytes);
        var data = reader.ReadFusionSerializable<FlashlightToggleData>();

        var entity = NetworkEntityManager.IdManager.RegisteredEntities.GetEntity(data.entityId);

        if (entity == null)
        {
            return;
        }

        var extender = entity.GetExtender<PropFlashlightExtender>();

        if (extender == null)
        {
            return;
        }

        var flashlight = extender.Component;
        flashlight.lightOn = !data.isEnabled;

        PropFlashlightPatches.IgnorePatches = true;

        flashlight.SwitchLight();

        PropFlashlightPatches.IgnorePatches = false;
    }
}