﻿using LabFusion.Data;
using LabFusion.Player;
using LabFusion.Utilities;
using LabFusion.Voice;

namespace LabFusion.Network;

public class PlayerVoiceChatData : IFusionSerializable, IDisposable
{
    public const int Size = sizeof(byte);

    public byte smallId;
    public byte[] bytes;

    public void Serialize(FusionWriter writer)
    {
        writer.Write(smallId);
        writer.Write(bytes);
    }

    public void Deserialize(FusionReader reader)
    {
        smallId = reader.ReadByte();
        bytes = reader.ReadBytes();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        ByteRetriever.Return(bytes);
    }

    public static PlayerVoiceChatData Create(byte smallId, byte[] voiceData)
    {
        return new PlayerVoiceChatData()
        {
            smallId = smallId,
            bytes = voiceData,
        };
    }
}

public class PlayerVoiceChatMessage : FusionMessageHandler
{
    public override byte Tag => NativeMessageTag.PlayerVoiceChat;

    public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
    {
        using var reader = FusionReader.Create(bytes);
        using var data = reader.ReadFusionSerializable<PlayerVoiceChatData>();

        // Check if voice chat is active
        if (!VoiceInfo.IsVoiceEnabled)
        {
            return;
        }

        // Read the voice chat
        var id = PlayerIdManager.GetPlayerId(data.smallId);

        if (id != null)
        {
            VoiceHelper.OnVoiceDataReceived(id, data.bytes);
        }

        // Bounce the message back
        if (NetworkInfo.IsServer)
        {
            using var message = FusionMessage.Create(Tag, bytes);
            MessageSender.BroadcastMessageExcept(data.smallId, NetworkChannel.Unreliable, message);
        }
    }
}