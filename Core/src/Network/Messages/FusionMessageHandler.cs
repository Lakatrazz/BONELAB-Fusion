using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using LabFusion.Utilities;
using LabFusion.Extensions;
using MelonLoader;

namespace LabFusion.Network
{
    public abstract class FusionMessageHandlerAsync : FusionMessageHandler {
        public abstract Task HandleMessageAsync(byte[] bytes, bool isServerHandled = false);

        public sealed override void HandleMessage(byte[] bytes, bool isServerHandled = false) {
            throw new NotImplementedException();
        }

        protected override IEnumerator HandleMessage_Internal(byte[] bytes, bool isServerHandled = false)
        {
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
                    yield break;
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

            if (awaitable != null)
            {
                while (!awaitable.CanContinue())
                    yield return null;
            }

            Task task = null;

            try
            {
                // Now handle the message info
                task = HandleMessageAsync(bytes, isServerHandled);
            }
            catch (Exception e)
            {
                FusionLogger.LogException("handling message", e);
            }

            // Await the async handle message before we return our byte buffer
            if (task != null) {
                while (!task.IsCompleted)
                    yield return null;
            }

            // Return the buffer
            ByteRetriever.Return(bytes);
        }

    }

    public abstract class FusionMessageHandler
    {
        public virtual byte? Tag { get; } = null;

        public Net.NetAttribute[] NetAttributes { get; set; }

        protected virtual IEnumerator HandleMessage_Internal(byte[] bytes, bool isServerHandled = false) {
            // Initialize the attribute info
            for (var i = 0; i < NetAttributes.Length; i++) {
                var attribute = NetAttributes[i];
                attribute.OnHandleBegin();
            }

            // Check if we should already stop handling
            for (var i = 0; i < NetAttributes.Length; i++) {
                var attribute = NetAttributes[i];

                if (attribute.StopHandling())
                    yield break;
            }

            // Check for any awaitable attributes
            Net.NetAttribute awaitable = null;

            for (var i = 0; i < NetAttributes.Length; i++) {
                var attribute = NetAttributes[i];

                if (attribute.IsAwaitable()) {
                    awaitable = attribute;
                    break;
                }
            }

            if (awaitable != null) {
                while (!awaitable.CanContinue())
                    yield return null;
            }

            try {
                // Now handle the message info
                HandleMessage(bytes, isServerHandled);
            }
            catch (Exception e) {
                FusionLogger.LogException("handling message", e);
            }

            // Return the buffer
            ByteRetriever.Return(bytes);
        }

        public abstract void HandleMessage(byte[] bytes, bool isServerHandled = false);

        // Handlers are created up front, they're not static
        public static void RegisterHandlersFromAssembly(Assembly targetAssembly)
        {
            if (targetAssembly == null) throw new NullReferenceException("Can't register from a null assembly!");

            FusionLogger.Log($"Populating MessageHandler list from {targetAssembly.GetName().Name}!");

            AssemblyUtilities.LoadAllValid<FusionMessageHandler>(targetAssembly, RegisterHandler);
        }

        public static void RegisterHandler<T>() where T : FusionMessageHandler => RegisterHandler(typeof(T));

        protected static void RegisterHandler(Type type)
        {
            FusionMessageHandler handler = Activator.CreateInstance(type) as FusionMessageHandler;

            if (handler.Tag == null)
            {
                FusionLogger.Warn($"Didn't register {type.Name} because its message index was null!");
            }
            else
            {
                handler.NetAttributes = type.GetCustomAttributes<Net.NetAttribute>().ToArray();

                byte index = handler.Tag.Value;

                if (Handlers[index] != null) throw new Exception($"{type.Name} has the same index as {Handlers[index].GetType().Name}, we can't replace handlers!");

                FusionLogger.Log($"Registered {type.Name}");

                Handlers[index] = handler;
            }
        }

        public static unsafe void ReadMessage(byte* buffer, int size, bool isServerHandled = false)
        {
            NetworkInfo.BytesDown += size;

            try
            {
                byte tag = buffer[0];
                byte[] message = ByteRetriever.Rent(size - 1);
                
                for (var i = 0; i < message.Length; i++)
                    message[i] = buffer[i + 1];

                if (Handlers[tag] != null)
                    MelonCoroutines.Start(Handlers[tag].HandleMessage_Internal(message, isServerHandled));
#if DEBUG
                else {
                    FusionLogger.Warn($"Received message with invalid tag {tag}!");
                }
#endif
            }
            catch (Exception e)
            {
                FusionLogger.Error($"Failed handling network message with reason: {e.Message}\nTrace:{e.StackTrace}");
            }
        }


        public static readonly FusionMessageHandler[] Handlers = new FusionMessageHandler[byte.MaxValue];
    }
}
