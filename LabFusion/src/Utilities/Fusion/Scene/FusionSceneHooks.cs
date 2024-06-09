namespace LabFusion.Utilities
{
    public static partial class FusionSceneManager
    {
        private static Action _onTargetLevelLoad = null;

        private static Action _onLevelLoad = null;

        private static Action _onDelayedLevelLoad = null;

        /// <summary>
        /// Waits until the server target level has loaded to invoke an action.
        /// If there is no server, it just waits until level load.
        /// </summary>
        /// <param name="action"></param>
        public static void HookOnTargetLevelLoad(Action action)
        {
            if (!HasTargetLoaded())
            {
                _onTargetLevelLoad += action;
            }
            else
            {
                action?.Invoke();
            }
        }

        /// <summary>
        /// Waits until the level has loaded to invoke an action.
        /// </summary>
        /// <param name="action"></param>
        public static void HookOnLevelLoad(Action action)
        {
            if (IsLoading())
            {
                _onLevelLoad += action;
            }
            else
            {
                action?.Invoke();
            }
        }

        /// <summary>
        /// Waits until the level has loaded with a bit of delay to invoke an action.
        /// </summary>
        /// <param name="action"></param>
        public static void HookOnDelayedLevelLoad(Action action)
        {
            if (IsDelayedLoading())
            {
                _onDelayedLevelLoad += action;
            }
            else
            {
                action?.Invoke();
            }
        }
    }
}
