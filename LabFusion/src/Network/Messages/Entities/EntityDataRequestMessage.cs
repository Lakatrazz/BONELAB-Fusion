using LabFusion.Player;

namespace LabFusion.Network;

public class EntityDataRequestMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.EntityDataRequest;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<EntityPlayerData>();

        var playerId = PlayerIDManager.GetPlayerID(data.PlayerID);

        if (playerId == null)
        {
            return;
        }

        CatchupManager.InvokeEntityDataCatchup(playerId, data.Entity);
    }
}