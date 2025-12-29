using LabFusion.Network.Serialization;
using LabFusion.Player;
using LabFusion.Voice;

namespace LabFusion.Network;

public class PlayerVoiceChatData : INetSerializable
{
    public byte[] Bytes;

    public int? GetSize()
    {
        return sizeof(int) + sizeof(byte) * Bytes.Length;
    }

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref Bytes);
    }
}

public class PlayerVoiceChatMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.PlayerVoiceChat;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<PlayerVoiceChatData>();

        var sender = received.Sender;

        if (!sender.HasValue)
        {
            return;
        }

        // Check if voice chat is active
        if (VoiceInfo.IsDeafened)
        {
            return;
        }

        // Read the voice chat
        var id = PlayerIDManager.GetPlayerID(sender.Value);

        if (id != null)
        {
            VoiceHelper.OnVoiceDataReceived(id, data.Bytes);
        }
    }
}