using LabFusion.Bonelab.Patching;
using LabFusion.Entities;
using LabFusion.Network;
using LabFusion.SDK.Modules;
using LabFusion.Bonelab.Extenders;
using LabFusion.Network.Serialization;

namespace LabFusion.Bonelab;

public class FlashlightToggleData : INetSerializable
{
    public const int Size = sizeof(byte) * 2 + sizeof(ushort);

    public ushort entityId;
    public bool isEnabled;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref entityId);
        serializer.SerializeValue(ref isEnabled);
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
        var data = received.ReadData<FlashlightToggleData>();

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