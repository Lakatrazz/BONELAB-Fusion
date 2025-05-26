using LabFusion.Player;
using LabFusion.Utilities;
using LabFusion.UI.Popups;

using Il2CppSLZ.Marrow.SceneStreaming;
using Il2CppSLZ.Marrow.Warehouse;

using UnityEngine;

using LabFusion.Network.Serialization;

namespace LabFusion.Network;

public class LevelRequestData : INetSerializable
{
    public byte smallId;
    public string barcode;
    public string title;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref smallId);
        serializer.SerializeValue(ref barcode);
        serializer.SerializeValue(ref title);
    }

    public static LevelRequestData Create(byte smallId, string barcode, string title)
    {
        return new LevelRequestData()
        {
            smallId = smallId,
            barcode = barcode,
            title = title,
        };
    }
}

public class LevelRequestMessage : NativeMessageHandler
{
    private const float _requestCooldown = 10f;
    private static float _timeOfRequest = -1000f;

    public override byte Tag => NativeMessageTag.LevelRequest;

    public override ExpectedReceiverType ExpectedReceiver => ExpectedReceiverType.ServerOnly;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        // Prevent request spamming
        if (TimeUtilities.TimeSinceStartup - _timeOfRequest <= _requestCooldown)
        {
            return;
        }

        _timeOfRequest = TimeUtilities.TimeSinceStartup;

        var data = received.ReadData<LevelRequestData>();

        // Get player and their username
        var id = PlayerIDManager.GetPlayerID(data.smallId);

        if (id != null && id.TryGetDisplayName(out var name))
        {
            Notifier.Send(new Notification()
            {
                Title = $"{data.title} Load Request",
                Message = new NotificationText($"{name} has requested to load {data.title}.", Color.yellow),

                SaveToMenu = true,
                ShowPopup = true,
                OnAccepted = () =>
                {
                    SceneStreamer.Load(new Barcode(data.barcode));
                },
            });
        }
    }
}