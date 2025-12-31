using LabFusion.Player;

namespace LabFusion.Network;

public class EntityDataRequestMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.EntityDataRequest;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<EntityPlayerData>();

        var playerID = PlayerIDManager.GetPlayerID(data.PlayerID);

        if (playerID == null)
        {
            return;
        }

        CatchupManager.InvokeEntityDataCatchup(playerID, data.Entity);
    }
}