using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using LabFusion.Utilities;
using LabFusion.Extensions;

using MelonLoader;

namespace LabFusion.Network
{
    public abstract class ModuleMessageHandler {
        internal ushort? _tag = null;
        public ushort? Tag => _tag;

        public Net.NetAttribute[] NetAttributes { get; set; }

        private IEnumerator HandleMessage_Internal(byte[] bytes, bool isServerHandled = false)
        {
            // Initialize the attribute info
            foreach (var attribute in NetAttributes) {
                attribute.OnHandleBegin();
            }

            // Check if we should already stop handling
            foreach (var attribute in NetAttributes)
            {
                if (attribute.StopHandling())
                    yield break;
            }

            // Check for any awaitable attributes
            Net.NetAttribute awaitable = null;

            foreach (var attribute in NetAttributes)
            {
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

            try {
                // Now handle the message info
                HandleMessage(bytes, isServerHandled);
            } 
            catch (Exception e) {
                FusionLogger.LogException("handling module message", e);
            }

            // Return the byte pool
            ByteRetriever.Return(bytes);
        }

        public abstract void HandleMessage(byte[] bytes, bool isServerHandled = false);

        public static void LoadHandlers(Assembly assembly) {
            if (assembly == null) 
                throw new NullReferenceException("Tried loading handlers from a null module assembly!");

            AssemblyUtilities.LoadAllValid<ModuleMessageHandler>(assembly, RegisterHandler);
        }

        public static void RegisterHandler<T>() where T : ModuleMessageHandler => RegisterHandler(typeof(T));

        protected static void RegisterHandler(Type type) {
            if (HandlerTypes.Contains(type))
                throw new ArgumentException($"Handler {type.Name} was already registered.");

            HandlerTypes.Add(type);
        }

        public static string[] GetExistingTypeNames() {
            string[] array = new string[HandlerTypes.Count];
            for (var i = 0; i < array.Length; i++) {
                array[i] = HandlerTypes[i].AssemblyQualifiedName;
            }
            return array;
        }

        public static void PopulateHandlerTable(string[] names) {
            Handlers = new ModuleMessageHandler[names.Length];

            for (ushort i = 0; i < names.Length; i++) {
                var type = Type.GetType(names[i]);
                if (type != null && HandlerTypes.Contains(type)) {
                    var handler = Internal_CreateHandler(type, i);
                    Handlers[i] = handler;
                }
            }
        }

        public static void ClearHandlerTable() {
            Handlers = null;
        }

        private static ModuleMessageHandler Internal_CreateHandler(Type type, ushort tag) {
            var handler = Activator.CreateInstance(type) as ModuleMessageHandler;
            handler._tag = tag;
            handler.NetAttributes = type.GetCustomAttributes<Net.NetAttribute>().ToArray();
            return handler;
        }

        public static ushort? GetHandlerTag(Type type) {
            if (Handlers != null) {
                for (ushort i = 0; i < Handlers.Length; i++) {
                    var other = Handlers[i];
                    if (other.GetType() == type)
                        return i;
                }
            }

            return null;
        }

        public static void ReadMessage(byte[] bytes, bool isServerHandled = false)
        {
            try
            {
                ushort tag = BitConverter.ToUInt16(bytes, 0);
                byte[] buffer = ByteRetriever.Rent(bytes.Length - 2);

                for (var i = 0; i < buffer.Length; i++)
                    buffer[i] = bytes[i + 2];

                // Since modules cannot be assumed to exist for everyone, we need to null check
                if (Handlers.Length > tag && Handlers[tag] != null) {
                    MelonCoroutines.Start(Handlers[tag].HandleMessage_Internal(buffer, isServerHandled));
                }
            }
            catch (Exception e)
            {
                FusionLogger.Error($"Failed handling network message with reason: {e.Message}\nTrace:{e.StackTrace}");
            }
        }

        internal static readonly List<Type> HandlerTypes = new List<Type>();
        internal static ModuleMessageHandler[] Handlers = null;
    }
}
