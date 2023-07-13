using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using LabFusion.Utilities;
using LabFusion.Extensions;
using LabFusion.Data;
using LabFusion.Network;
using BoneLib;
using SLZ.Interaction;
using UnityEngine; // can prob remove before pull request
using LabFusion.Preferences;

namespace LabFusion.Data { 
    public abstract class LevelDataHandler {
        public static List<Component> buttonToggle; // should change to new spot
        protected virtual void SceneAwake() { }
        protected virtual void MainSceneInitialized() { }
        protected virtual void PlayerCatchup(ulong longId) { }

        private static void OnSceneAwake() {
            for (var i = 0; i < Handlers.Count; i++) {
                Handlers[i].SceneAwake();
            }
            var objectsWithKeyword = Transform.FindObjectsOfType<Transform>(true);
            foreach (Transform obj in objectsWithKeyword)
            {
                if (obj.name.Contains("FLOORS") || obj.name.Contains("LoadButtons"))
                {
                    for (int i = 0; i < obj.childCount; i++)
                    {
                        if (i == 10 || i == 11)
                        {
                            continue;
                        }
                        Transform child = obj.GetChild(i);
                        SLZ.Interaction.ButtonToggle ButtonToggle = child.GetComponent<SLZ.Interaction.ButtonToggle>();
                        if (ButtonToggle != null)
                        {
                            if (FusionPreferences.LocalServerSettings.LevelSwitchingButtonsEnabled.Equals(true)) //chances are theres a better way to do this but this works
                            {
                                ButtonToggle.enabled = false;
                            }
                            else
                            {
                                ButtonToggle.enabled = true;
                            }
                        }
                    }
                }
            }
        }

        private static void OnMainSceneInitialized() {
            for (var i = 0; i < Handlers.Count; i++) {
                Handlers[i].MainSceneInitialized();
            }
        }

        private static void OnPlayerCatchup(ulong longId) {
            for (var i = 0; i < Handlers.Count; i++) {
                Handlers[i].PlayerCatchup(longId);
            }
        }

        public static void OnInitializeMelon() {
            // Hook functions
            Hooking.OnLevelInitialized += (_) => { OnSceneAwake(); };
            MultiplayerHooking.OnMainSceneInitialized += OnMainSceneInitialized;
            MultiplayerHooking.OnPlayerCatchup += OnPlayerCatchup;

            // Register all of our handlers
            RegisterHandlersFromAssembly(FusionMod.FusionAssembly);
        }

        public static void RegisterHandlersFromAssembly(Assembly targetAssembly)
        {
            if (targetAssembly == null) throw new NullReferenceException("Can't register from a null assembly!");

            FusionLogger.Log($"Populating LevelDataHandler list from {targetAssembly.GetName().Name}!");

            AssemblyUtilities.LoadAllValid<LevelDataHandler>(targetAssembly, RegisterHandler);
        }

        public static void RegisterHandler<T>() where T : FusionMessageHandler => RegisterHandler(typeof(T));

        protected static void RegisterHandler(Type type)
        {
            // Create the handler
            LevelDataHandler handler = Activator.CreateInstance(type) as LevelDataHandler;
            Handlers.Add(handler);

            FusionLogger.Log($"Registered {type.Name}");
        }

        public static readonly List<LevelDataHandler> Handlers = new List<LevelDataHandler>();
    }
}
