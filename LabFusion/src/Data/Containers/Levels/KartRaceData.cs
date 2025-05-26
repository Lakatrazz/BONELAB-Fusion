using UnityEngine;

using LabFusion.Network;
using LabFusion.Player;
using LabFusion.Utilities;
using LabFusion.Scene;

using Il2CppSLZ.Marrow.Warehouse;
using Il2CppSLZ.Bonelab;
using Il2CppSLZ.Marrow.VoidLogic;

namespace LabFusion.Data;

public class KartRaceData : LevelDataHandler
{
    public override string LevelTitle => "10 - Monogon Motorway";

    public const string GokartName = "CrateSpawner (Gokart)";

    public const string ResetButtonName = "button_3x_Toggle_Powered_RESETRACE";

    private static readonly Vector3[] ExtraKartPositions = new Vector3[5]
    {
        new(-3.62f, -5.222564f, 6.46f),
        new(-1.61f, -5.222564f, 3.77f),
        new(0.89f, -5.222564f, 6.29f),
        new(3.01f, -5.222564f, 3.62f),
        new(5.85f, -5.222564f, 5.91f),
    };

    public static GameControl_KartRace GameController;

    protected override void MainSceneInitialized()
    {
        GameController = GameObject.FindObjectOfType<GameControl_KartRace>();
        SpawnExtraKarts();
    }

    private static void SpawnExtraKarts()
    {
        // Make sure we have a server
        if (!NetworkInfo.HasServer)
        {
            return;
        }

        // If we are in Monogon Motorway, spawn extra go-karts
        // Link these spawns up to the reset race button too
        var extraPlayers = PlayerIDManager.PlayerCount - 1;

        // Get the gokart spawner
        GameObject gokartSpawner = GameObject.Find(GokartName);

        // Get the reset button
        var resetButtonGo = GameObject.Find(ResetButtonName);

        EventAdapter resetEventAdapter = null;

        if (resetButtonGo != null)
        {
            resetEventAdapter = resetButtonGo.GetComponentInChildren<EventAdapter>();
        }

        // Now actually create each go kart
        if (gokartSpawner)
        {
            for (var i = 0; i < extraPlayers && i < ExtraKartPositions.Length; i++)
            {
                // Create placers
                var newPlacer = GameObject.Instantiate(gokartSpawner);
                newPlacer.transform.parent = gokartSpawner.transform.parent;
                newPlacer.transform.localPosition = ExtraKartPositions[i];
                newPlacer.name = $"{gokartSpawner.name} Player {i + 1}";
                var newPlacerScript = newPlacer.GetComponent<CrateSpawner>();

                // Spawn the spawnable from the new placer
                FusionSceneManager.HookOnDelayedLevelLoad(newPlacerScript.SpawnSpawnable);

                // Add to the reset button event
                if (resetEventAdapter != null)
                {
                    var inputRoseEvent = (EventAdapter adapter, IVoidLogicSource source, float value) =>
                    {
                        newPlacerScript.SpawnSpawnable();
                    };

                    resetEventAdapter.InputRose.add_DynamicCalls(inputRoseEvent);
                }
            }
        }
        else
        {
            FusionLogger.Warn($"Could not find the Gokart spawner in Monogon Motorway, please report this to {FusionMod.ModAuthor}!");
        }
    }
}