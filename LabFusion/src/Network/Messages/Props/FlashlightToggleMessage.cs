﻿using LabFusion.Data;
using LabFusion.Patching;
using LabFusion.Entities;

namespace LabFusion.Network;

public class FlashlightToggleData : IFusionSerializable
{
    public const int Size = sizeof(byte) * 2 + sizeof(ushort);

    public byte smallId;
    public ushort syncId;
    public bool isEnabled;

    public void Serialize(FusionWriter writer)
    {
        writer.Write(smallId);
        writer.Write(syncId);
        writer.Write(isEnabled);
    }

    public void Deserialize(FusionReader reader)
    {
        smallId = reader.ReadByte();
        syncId = reader.ReadUInt16();
        isEnabled = reader.ReadBoolean();
    }

    public static FlashlightToggleData Create(byte smallId, ushort syncId, bool isEnabled)
    {
        return new FlashlightToggleData()
        {
            smallId = smallId,
            syncId = syncId,
            isEnabled = isEnabled,
        };
    }
}

[Net.DelayWhileTargetLoading]
public class FlashlightToggleMessage : FusionMessageHandler
{
    public override byte Tag => NativeMessageTag.FlashlightToggle;

    public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
    {
        using FusionReader reader = FusionReader.Create(bytes);
        var data = reader.ReadFusionSerializable<FlashlightToggleData>();

        // Send message to other clients if server
        if (isServerHandled)
        {
            using var message = FusionMessage.Create(Tag, bytes);
            MessageSender.BroadcastMessageExcept(data.smallId, NetworkChannel.Reliable, message, false);

            return;
        }

        var entity = NetworkEntityManager.IdManager.RegisteredEntities.GetEntity(data.syncId);

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