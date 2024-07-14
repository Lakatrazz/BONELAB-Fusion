﻿using UnityEngine;

using Il2CppSLZ.Bonelab;
using Il2CppSLZ.Vehicle;
using Il2CppSLZ.Marrow.Interaction;

using LabFusion.Utilities;
using LabFusion.Extensions;
using LabFusion.Network;
using LabFusion.Player;
using LabFusion.SDK.Achievements;

namespace LabFusion.Data;

public class HomeData : LevelDataHandler
{
    public override string LevelTitle => "14 - Home";

    public static GameControl_Outro GameController;
    public static TaxiController TaxiController;
    public static Seat TaxiSeat;
    public static ArticulatedArmController ArmController;
    public static ArmFinale ArmFinale;

    protected override void MainSceneInitialized()
    {
        GameController = GameObject.FindObjectOfType<GameControl_Outro>(true);
        if (GameController != null)
        {
            // In a server, teleport the player to the top of the lift so they don't spawn underneath it if its synced
            if (NetworkInfo.HasServer)
            {
                FusionPlayer.Teleport(new Vector3(-9.030009f, -5.142975f, -71.18999f), Vector3Extensions.forward, true);
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
        if (NetworkInfo.HasServer && PlayerIdManager.HasOtherPlayers)
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
            var rm = RigData.RigReferences.RigManager;
            var pos = new Vector3(-0.25f, 95.23f, 13f);

            rm.Teleport(pos, true);
            rm.physicsRig.ResetHands(Handedness.BOTH);
        }
    }
}