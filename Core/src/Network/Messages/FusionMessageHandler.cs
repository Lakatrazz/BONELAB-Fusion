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
    public abstract class FusionMessageHandler
    {
        public virtual byte? Tag { get; } = null;

        public Net.NetAttribute[] NetAttributes { get; set; }

        private IEnumerator HandleMessage_Internal(byte[] bytes, bool isServerHandled = false) {
            // Initialize the attribute info
            foreach (var attribute in NetAttributes) {
                attribute.OnHandleBegin();
            }

            // Check if we should already stop handling
            foreach (var attribute in NetAttributes) {
                if (attribute.StopHandling())
                    yield break;
            }

            // Check for any awaitable attributes
            Net.NetAttribute awaitable = null;

            foreach (var attribute in NetAttributes) {
                if (attribute.IsAwaitable()) {
                    awaitable = attribute;
                    break;
                }
            }

            if (awaitable != null) {
                while (!awaitable.CanContinue())
                    yield return null;
            }

            // Now handle the message info
            HandleMessage(bytes, isServerHandled);
        }

        public abstract void HandleMessage(byte[] bytes, bool isServerHandled = false);

        // Handlers are created up front, they're not static
        public static void RegisterHandlersFromAssembly(Assembly targetAssembly)
        {
            if (targetAssembly == null) throw new NullReferenceException("Can't register from a null assembly!");

            FusionLogger.Log($"Populating MessageHandler list from {targetAssembly.GetName().Name}!");

            // I am aware LINQ is kinda gross but this is works!
            targetAssembly.GetTypes()
                .Where(type => typeof(FusionMessageHandler).IsAssignableFrom(type) && !type.IsAbstract)
                .ForEach(type => {
                    try
                    {
                        RegisterHandler(type);
                    }
                    catch (Exception e)
                    {
                        FusionLogger.Error(e.Message);
                    }
                });
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

        public static void ReadMessage(byte[] bytes, bool isServerHandled = false)
        {
            try
            {
                byte tag = bytes[0];
                byte[] buffer = new byte[bytes.Length - 1];

                for (var i = 0; i < buffer.Length; i++)
                    buffer[i] = bytes[i + 1];

                MelonCoroutines.Start(Handlers[tag].HandleMessage_Internal(buffer, isServerHandled));
            }
            catch (Exception e)
            {
                FusionLogger.Error($"Failed handling network message with reason: {e.Message}\nTrace:{e.StackTrace}");
            }
        }


        public static readonly FusionMessageHandler[] Handlers = new FusionMessageHandler[byte.MaxValue];
    }
}
