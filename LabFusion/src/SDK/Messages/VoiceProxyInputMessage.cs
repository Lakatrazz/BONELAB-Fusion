using LabFusion.Marrow.Integration;
using LabFusion.Network;
using LabFusion.SDK.Extenders;
using LabFusion.Network.Serialization;
using LabFusion.SDK.Modules;

namespace LabFusion.SDK.Messages;

public class VoiceProxyInputData : INetSerializable
{
    public int? GetSize() => ComponentPathData.Size + sizeof(byte) + sizeof(bool);

    public ComponentPathData ComponentData;

    public byte PlayerID;

    public bool Input;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref ComponentData);
        serializer.SerializeValue(ref PlayerID);
        serializer.SerializeValue(ref Input);
    }
}

public class VoiceProxyInputMessage : ModuleMessageHandler
{
    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<VoiceProxyInputData>();

        if (data.ComponentData.TryGetComponent<VoiceProxy, VoiceProxyExtender>(VoiceProxy.HashTable, out var voiceProxy))
        {
            OnFoundVoiceProxy(voiceProxy, data);
        }
    }

    private static void OnFoundVoiceProxy(VoiceProxy proxy, VoiceProxyInputData data)
    {
        if (data.Input)
        {
            proxy.InputID = data.PlayerID;
        }
        else
        {
            proxy.InputID = null;
        }
    }
}
