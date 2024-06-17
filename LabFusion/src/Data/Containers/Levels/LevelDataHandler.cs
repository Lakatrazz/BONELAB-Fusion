using System.Reflection;

using LabFusion.Utilities;
using LabFusion.Network;

using BoneLib;
using LabFusion.Representation;

namespace LabFusion.Data
{
    public abstract class LevelDataHandler
    {
        public virtual string LevelTitle => null;

        protected virtual bool IsMatchingScene()
        {
            if (string.IsNullOrEmpty(LevelTitle))
                return true;

            return FusionSceneManager.Title == LevelTitle && FusionSceneManager.Level.Pallet.Internal;
        }

        protected virtual void SceneAwake() { }
        protected virtual void MainSceneInitialized() { }
        protected virtual void PlayerCatchup(PlayerId playerId) { }

        private static void OnSceneAwake()
        {
            for (var i = 0; i < Handlers.Count; i++)
            {
                var handler = Handlers[i];

                if (handler.IsMatchingScene())
                    handler.SceneAwake();
            }
        }

        private static void OnMainSceneInitialized()
        {
            for (var i = 0; i < Handlers.Count; i++)
            {
                var handler = Handlers[i];

                if (handler.IsMatchingScene())
                    handler.MainSceneInitialized();
            }
        }

        private static void OnPlayerCatchup(PlayerId playerId)
        {
            for (var i = 0; i < Handlers.Count; i++)
            {
                Handlers[i].PlayerCatchup(playerId);
            }
        }

        public static void OnInitializeMelon()
        {
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

#if DEBUG
            FusionLogger.Log($"Populating LevelDataHandler list from {targetAssembly.GetName().Name}!");
#endif

            AssemblyUtilities.LoadAllValid<LevelDataHandler>(targetAssembly, RegisterHandler);
        }

        public static void RegisterHandler<T>() where T : FusionMessageHandler => RegisterHandler(typeof(T));

        protected static void RegisterHandler(Type type)
        {
            // Create the handler
            LevelDataHandler handler = Activator.CreateInstance(type) as LevelDataHandler;
            Handlers.Add(handler);

#if DEBUG
            FusionLogger.Log($"Registered {type.Name}");
#endif
        }

        public static readonly List<LevelDataHandler> Handlers = new();
    }
}
