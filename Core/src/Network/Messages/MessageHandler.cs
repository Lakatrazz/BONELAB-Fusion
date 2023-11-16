using LabFusion.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Network
{
    public abstract class MessageHandler
    {
        public Net.NetAttribute[] NetAttributes { get; set; }

        protected virtual void Internal_HandleMessage(byte[] bytes, bool isServerHandled = false)
        {
            // If there are no attributes, just handle the message
            if (NetAttributes.Length <= 0)
            {
                Internal_FinishMessage(bytes, isServerHandled);
                return;
            }

            // Initialize the attribute info
            for (var i = 0; i < NetAttributes.Length; i++)
            {
                var attribute = NetAttributes[i];
                attribute.OnHandleBegin();
            }

            // Check if we should already stop handling
            for (var i = 0; i < NetAttributes.Length; i++)
            {
                var attribute = NetAttributes[i];

                if (attribute.StopHandling())
                    return;
            }

            // Check for any awaitable attributes
            Net.NetAttribute awaitable = null;

            for (var i = 0; i < NetAttributes.Length; i++)
            {
                var attribute = NetAttributes[i];

                if (attribute.IsAwaitable())
                {
                    awaitable = attribute;
                    break;
                }
            }

            // Hook the awaitable attribute so that we can handle the message when its ready
            if (awaitable != null)
            {
                awaitable.HookComplete(() => { Internal_FinishMessage(bytes, isServerHandled); });
            }
            else
                Internal_FinishMessage(bytes, isServerHandled);
        }

        protected virtual void Internal_FinishMessage(byte[] bytes, bool isServerHandled = false)
        {
            try
            {
                // Now handle the message info
                HandleMessage(bytes, isServerHandled);
            }
            catch (Exception e)
            {
                FusionLogger.LogException("handling message", e);
            }

            // Return the buffer
            ByteRetriever.Return(bytes);
        }

        public abstract void HandleMessage(byte[] bytes, bool isServerHandled = false);

    }
}
