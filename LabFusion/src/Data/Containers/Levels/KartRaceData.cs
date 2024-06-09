using UnityEngine;

using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.Utilities;

using Il2CppSLZ.Interaction;
using Il2CppSLZ.Marrow.Warehouse;
using Il2CppSLZ.Bonelab;

using UnityEngine.Events;

namespace LabFusion.Data
{
    public class KartRaceData : LevelDataHandler
    {
        public override string LevelTitle => "10 - Monogon Motorway";

        private static readonly Vector3[] ExtraKartPositions = new Vector3[5]
        {
            new Vector3(-3.62f, -5.222564f, 6.46f),
            new Vector3(-1.61f, -5.222564f, 3.77f),
            new Vector3(0.89f, -5.222564f, 6.29f),
            new Vector3(3.01f, -5.222564f, 3.62f),
            new Vector3(5.85f, -5.222564f, 5.91f),
        };

        public static GameControl_KartRace GameController;

        protected override void MainSceneInitialized()
        {
            GameController = GameObject.FindObjectOfType<GameControl_KartRace>();
            SpawnExtraKarts();
        }

        private static void SpawnExtraKarts()
        {
            // If we are in Monogon Motorway, spawn extra go-karts
            // Link these spawns up to the reset race button too
            if (GameController != null && NetworkInfo.HasServer)
            {
                var extraPlayers = PlayerIdManager.PlayerCount - 1;

                // Get the go kart placer
                GameObject gokartPlacer = GameObject.Find("Spawnable Placer (Gokart)");

                // Get the reset button
                var resetButtonGo = GameObject.Find("prop_bigButton_RESETRACE");

                ButtonToggle resetButton = null;
                if (resetButtonGo != null)
                {
                    resetButton = resetButtonGo.GetComponent<ButtonToggle>();
                }

                // Now actually create each go kart
                if (gokartPlacer)
                {
                    for (var i = 0; i < extraPlayers && i < ExtraKartPositions.Length; i++)
                    {
                        // Create placers
                        var newPlacer = GameObject.Instantiate(gokartPlacer);
                        newPlacer.transform.parent = gokartPlacer.transform.parent;
                        newPlacer.transform.localPosition = ExtraKartPositions[i];
                        newPlacer.name = $"{gokartPlacer.name} Player {i + 1}";
                        var newPlacerScript = newPlacer.GetComponent<CrateSpawner>();

                        newPlacerScript.ReSpawnSpawnable();

                        // Add to reset button
                        if (resetButton != null)
                        {
                            resetButton.onDepress.AddListener((UnityAction)newPlacerScript.ReSpawnSpawnable);
                        }
                    }
                }
                else
                {
#if DEBUG
                    FusionLogger.Warn("Monogon Motorway is missing the go kart placer!");
#endif
                }
            }
        }
    }
}
