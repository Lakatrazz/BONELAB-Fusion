using UnityEngine;

using LabFusion.Network;
using LabFusion.Player;
using LabFusion.Utilities;
using LabFusion.Scene;
using LabFusion.SDK.Scene;

using Il2CppSLZ.Marrow.Warehouse;
using Il2CppSLZ.Bonelab;
using Il2CppSLZ.Marrow.VoidLogic;

namespace LabFusion.Bonelab.Scene;

public class MonogonMotorwayEventHandler : GamemodeLevelEventHandler
{
    public override string LevelBarcode => "SLZ.BONELAB.Content.Level.LevelKartRace";

    public override Vector3[] GamemodeMarkerPoints => new Vector3[] {
        new(49.9405f, 1.0374f, -51.5276f),
        new(39.0956f, 7.0375f, -24.3985f),
        new(34.3241f, 1.197f, -52.855f),
        new(-32.3943f, 10.9575f, 4.8938f),
        new(24.2696f, 4.1897f, 64.7375f),
        new(78.0983f, 6.4815f, 55.0796f),
        new(89.998f, 3.6803f, -2.1484f),
        new(43.7032f, 14.0374f, 14.1218f),
        new(2.5037f, 6.0376f, 68.6963f),
        new(26.7221f, 5.6433f, 24.9881f),
        new(-26.8878f, 6.1773f, -62.3645f),
        new(31.9522f, 0.3404f, -61.9676f),
        new(10.5809f, 7.5374f, -34.0675f),
        new(80.5201f, 7.2038f, -67.1719f),
    };

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

    protected override void OnLevelLoaded()
    {
        base.OnLevelLoaded();

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