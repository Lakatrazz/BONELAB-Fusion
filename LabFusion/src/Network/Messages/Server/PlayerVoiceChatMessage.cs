using LabFusion.Data;
using LabFusion.Player;
using LabFusion.Voice;

namespace LabFusion.Network;

public class PlayerVoiceChatData : IFusionSerializable
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

    public static PlayerVoiceChatData Create(byte smallId, byte[] voiceData)
    {
        return new PlayerVoiceChatData()
        {
            smallId = smallId,
            bytes = voiceData,
        };
    }
}

public class PlayerVoiceChatMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.PlayerVoiceChat;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<PlayerVoiceChatData>();

        // Check if voice chat is active
        if (VoiceInfo.IsDeafened)
        {
            return;
        }

        // Read the voice chat
        var id = PlayerIdManager.GetPlayerId(data.smallId);

        if (id != null)
        {
            VoiceHelper.OnVoiceDataReceived(id, data.bytes);
        }
    }
}