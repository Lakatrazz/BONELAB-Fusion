using LabFusion.Network.Serialization;
using LabFusion.Player;
using LabFusion.Voice;

namespace LabFusion.Network;

public class PlayerVoiceChatData : INetSerializable
{
    public byte smallId;
    public byte[] bytes;

    public int? GetSize()
    {
        return sizeof(byte) + sizeof(int) + sizeof(byte) * bytes.Length;
    }

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref smallId);
        serializer.SerializeValue(ref bytes);
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
        var id = PlayerIDManager.GetPlayerID(data.smallId);

        if (id != null)
        {
            VoiceHelper.OnVoiceDataReceived(id, data.bytes);
        }
    }
}