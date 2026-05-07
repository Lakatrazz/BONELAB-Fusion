using Il2CppSLZ.Bonelab;
using Il2CppSLZ.Marrow;

using MarrowFusion.Bonelab.Serialization;
using MarrowFusion.Bonelab.Messages;

using LabFusion.Entities;
using LabFusion.Network;
using LabFusion.Utilities;
using LabFusion.Player;
using LabFusion.Marrow;
using LabFusion.Marrow.Rig;

using UnityEngine;

namespace MarrowFusion.Bonelab.Extenders;

public static class BonelabPlayerManager
{
    public static void Initialize()
    {
        NetworkPlayer.OnNetworkPlayerRegistered += OnNetworkPlayerRegistered;
        MultiplayerHooking.OnMainSceneInitialized += OnMainSceneInitialized;
        MultiplayerHooking.OnPlayerJoined += OnPlayerJoined;
        MultiplayerHooking.OnJoinedServer += OnJoinedServer;
        RigStripper.OnStripRigManager += OnStripBonelabRigManager;

        MarrowGameReferences.CalibrationAvatarReference = BonelabAvatarReferences.PolyBlankReference;
        MarrowGameReferences.CalibrationAvatarHeight = MarrowConstants.StandardHeight;
    }

    public static void Uninitialize()
    {
        NetworkPlayer.OnNetworkPlayerRegistered -= OnNetworkPlayerRegistered;
        MultiplayerHooking.OnMainSceneInitialized -= OnMainSceneInitialized;
        MultiplayerHooking.OnPlayerJoined -= OnPlayerJoined;
        MultiplayerHooking.OnJoinedServer -= OnJoinedServer;
        RigStripper.OnStripRigManager -= OnStripBonelabRigManager;
    }

    private static void OnStripBonelabRigManager(RigManager rigManager)
    {
        // Remove UI inputs
        var controllerRig = rigManager.ControllerRig;

        GameObject.DestroyImmediate(controllerRig.leftController.GetComponent<UIControllerInput>());
        GameObject.DestroyImmediate(controllerRig.rightController.GetComponent<UIControllerInput>());
    }

    private static void OnNetworkPlayerRegistered(NetworkPlayer player)
    {
        BonelabNetworkPlayer.CreatePlayer(player.NetworkEntity, player);
    }

    private static void OnMainSceneInitialized()
    {
        PlayerRefs.Instance.PlayerBodyVitals.rescaleEvent += (BodyVitals.RescaleUI)OnVitalsChanged;
    }

    private static void OnPlayerJoined(PlayerID playerID) => OnVitalsChanged();

    private static void OnJoinedServer() => OnVitalsChanged();

    private static void OnVitalsChanged()
    {
        if (!NetworkInfo.HasServer)
        {
            return;
        }

        var bodyVitals = new SerializedBodyVitals(PlayerRefs.Instance.PlayerBodyVitals);

        MessageRelay.RelayModule<BodyVitalsMessage, SerializedBodyVitals>(bodyVitals, CommonMessageRoutes.ReliableToOtherClients);
    }
}
