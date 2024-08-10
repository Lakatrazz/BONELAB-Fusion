using LabFusion.Network;

namespace LabFusion.Senders
{
    public static class QuickSender
    {
        /// <summary>
        /// Calls an action if this is the server and returns true. Otherwise, returns false.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public static bool SendServerMessage(Action action)
        {
            if (NetworkInfo.HasServer)
            {
                if (NetworkInfo.IsServer)
                    action();
                else
                    return false;
            }

            return true;
        }
    }
}
