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

namespace LabFusion.Data {
    public abstract class LevelDataHandler {
        protected virtual void SceneAwake() { }
        protected virtual void MainSceneInitialized() { }
        protected virtual void PlayerCatchup(ulong longId) { }

        private static void OnSceneAwake() {
            for (var i = 0; i < Handlers.Count; i++) {
                Handlers[i].SceneAwake();
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
