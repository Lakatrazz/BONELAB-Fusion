using LabFusion.Entities;
using LabFusion.Extensions;
using LabFusion.Marrow.Messages;
using LabFusion.Network;
using LabFusion.Player;
using LabFusion.Scene;

namespace LabFusion.Marrow.Rig;

/// <summary>
/// Actions that can occur for a NetworkRig.
/// </summary>
public enum RigActionType
{
    /// <summary>
    /// No action was set.
    /// </summary>
    None,

    /// <summary>
    /// The NetworkRig jumped.
    /// </summary>
    Jump,

    /// <summary>
    /// The NetworkRig was killed, and is playing the death animation.
    /// </summary>
    Dying,

    /// <summary>
    /// The NetworkRig completed the death animation.
    /// </summary>
    Death,

    /// <summary>
    /// The NetworkRig saved itself and is no longer dying.
    /// </summary>
    Recovery,

    /// <summary>
    /// The NetworkRig died and respawned.
    /// </summary>
    Respawn,
}

public delegate void RigActionDelegate(NetworkRig networkRig, RigActionType type);

public delegate void PlayerRigActionDelegate(PlayerID playerID, RigActionType type);

public static class RigActionManager
{
    /// <summary>
    /// Invoked when a NetworkRig has relayed a RigAction from one of the <see cref="RigActionType"/>s.
    /// </summary>
    public static event RigActionDelegate RigActed;

    /// <summary>
    /// The same as <see cref="RigActed"/>, but only invoked when a NetworkPlayer has a rig action.
    /// </summary>
    public static event PlayerRigActionDelegate PlayerRigActed;

    public static void RelayRigAction(NetworkEntityReference rigReference, RigActionType type)
    {
        if (!NetworkSceneManager.IsLevelNetworked)
        {
            return;
        }

        var data = new RigActionData()
        {
            RigReference = rigReference,
            Type = type,
        };

        MessageRelay.RelayModule<RigActionMessage, RigActionData>(data, CommonMessageRoutes.ReliableToClients);
    }

    public static void OnRigAction(NetworkRig networkRig, RigActionType type)
    {
        RigActed?.InvokeSafe(networkRig, type, "executing RigActionManager.RigActed event");

        var networkEntity = networkRig.NetworkEntity;

        var networkPlayer = networkEntity.GetExtender<NetworkPlayer>();

        if (networkPlayer != null)
        {
            PlayerRigActed?.InvokeSafe(networkPlayer.PlayerID, type, "executing RigActionManager.PlayerActed event");
        }
    }
}
