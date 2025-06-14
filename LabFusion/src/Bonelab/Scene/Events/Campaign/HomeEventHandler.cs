using UnityEngine;

using Il2CppSLZ.Bonelab;
using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Interaction;

using LabFusion.Data;
using LabFusion.Extensions;
using LabFusion.Network;
using LabFusion.Player;
using LabFusion.SDK.Achievements;
using LabFusion.SDK.Scene;

namespace LabFusion.Bonelab.Scene;

public class HomeEventHandler : GamemodeLevelEventHandler
{
    public override string LevelBarcode => "SLZ.BONELAB.Content.Level.LevelOutro";

    public override Vector3[] GamemodeMarkerPoints => new Vector3[] {
        new(-1.12f, 12.018f, -48.6185f),
        new(-9.0939f, -5.2375f, -71.086f),
        new(12.9752f, -5.2126f, -51.2283f),
        new(2.0007f, -3.6254f, -56.3616f),
        new(-19.5654f, -5.2125f, -79.5254f),
        new(-20.3357f, -5.2125f, -46.3607f),
        new(18.6373f, 4.7875f, -43.5794f),
        new(9.7098f, -5.0691f, -77.7367f),
    };

    public static GameControl_Outro GameController;
    public static TaxiController TaxiController;
    public static Seat TaxiSeat;
    public static ArticulatedArmController ArmController;
    public static ArmFinale ArmFinale;

    protected override void OnLevelLoaded()
    {
        base.OnLevelLoaded();

        GameController = GameObject.FindObjectOfType<GameControl_Outro>(true);
        if (GameController != null)
        {
            // In a server, teleport the player to the top of the lift so they don't spawn underneath it if its synced
            if (NetworkInfo.HasServer)
            {
                LocalPlayer.TeleportToPosition(new Vector3(-9.030009f, -5.142975f, -71.18999f), Vector3Extensions.forward);
            }

            TaxiController = GameObject.FindObjectOfType<TaxiController>(true);
            TaxiSeat = TaxiController.rearSeat;

            ArmController = GameObject.FindObjectOfType<ArticulatedArmController>(true);
            ArmFinale = GameObject.FindObjectOfType<ArmFinale>(true);

            // Hook seat event
            TaxiSeat.RegisteredEvent += (Il2CppSystem.Action)OnTaxiSeatRegistered;

            // Add extra seats
            // Inside seat
            CreateSeat(2, new Vector3(-0.326f, 0.441f, -1.125f), Vector3Extensions.zero);

            // Trunk seats
            CreateSeat(3, new Vector3(0.48f, 0.928f, -2.138f), Vector3Extensions.up * -180f);
            CreateSeat(4, new Vector3(-0.48f, 0.928f, -2.138f), Vector3Extensions.up * -180f);

            // Hood seats
            CreateSeat(5, new Vector3(0.48f, 0.928f, 1.998f), Vector3Extensions.zero);
            CreateSeat(6, new Vector3(-0.48f, 0.928f, 1.998f), Vector3Extensions.zero);
        }
    }

    private static void OnTaxiSeatRegistered()
    {
        // Give the achievement in a server with more than 1 player
        if (NetworkInfo.HasServer && PlayerIDManager.HasOtherPlayers)
        {
            // Increment the achievement task
            if (AchievementManager.TryGetAchievement<OneMoreTime>(out var achievement))
                achievement.IncrementTask();
        }
    }

    private static void CreateSeat(int index, Vector3 localPosition, Vector3 localRotation)
    {
        var extraSeat = GameObject.Instantiate(TaxiSeat.gameObject);
        extraSeat.transform.parent = TaxiSeat.transform.parent;
        extraSeat.SetActive(true);
        extraSeat.name = $"Seat ({index})";

        extraSeat.transform.SetLocalPositionAndRotation(localPosition, Quaternion.Euler(localRotation));
    }

    public static void TeleportToJimmyFinger()
    {
        if (RigData.HasPlayer)
        {
            var rm = RigData.Refs.RigManager;
            var pos = new Vector3(-0.25f, 95.23f, 13f);

            rm.Teleport(pos, true);
            rm.physicsRig.ResetHands(Handedness.BOTH);
        }
    }
}