using LabFusion.Bonelab.Patching;
using LabFusion.Entities;
using LabFusion.Network;
using LabFusion.SDK.Modules;
using LabFusion.Bonelab.Extenders;
using LabFusion.Network.Serialization;

namespace LabFusion.Bonelab.Messages;

public class FlashlightToggleData : INetSerializable
{
    public const int Size = NetworkEntityReference.Size + sizeof(bool);

    public NetworkEntityReference FlashlightEntity;

    public bool LightOn;

    public int? GetSize() => Size;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref FlashlightEntity);
        serializer.SerializeValue(ref LightOn);
    }

    public static FlashlightToggleData Create(NetworkEntityReference flashlightEntity, bool lightOn)
    {
        return new FlashlightToggleData()
        {
            FlashlightEntity = flashlightEntity,
            LightOn = lightOn,
        };
    }
}

[Net.DelayWhileTargetLoading]
public class FlashlightToggleMessage : ModuleMessageHandler
{
    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<FlashlightToggleData>();

        if (!data.FlashlightEntity.TryGetEntity(out var entity))
        {
            return;
        }

        var extender = entity.GetExtender<PropFlashlightExtender>();

        if (extender == null)
        {
            return;
        }

        var flashlight = extender.Component;
        flashlight.lightOn = !data.LightOn;

        PropFlashlightPatches.IgnorePatches = true;

        flashlight.SwitchLight();
    }
}