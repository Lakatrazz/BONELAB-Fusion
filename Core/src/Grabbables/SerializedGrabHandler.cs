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

namespace LabFusion.Grabbables {
    public enum GrabGroup : byte {
        UNKNOWN = 0,
        PLAYER_BODY = 1,
        PROP = 2,
        NPC = 3,
        STATIC = 4,
        WORLD_GRIP = 5,
    }

    public abstract class GrabGroupHandler<T> : GrabGroupHandler where T : SerializedGrab, new() {
        public override void HandleGrab(ref SerializedGrab serializedGrab, FusionReader reader) {
            serializedGrab = reader.ReadFusionSerializable<T>();
        }
    }

    public abstract class GrabGroupHandler {
        public virtual GrabGroup? Group { get; } = null;

        public abstract void HandleGrab(ref SerializedGrab serializedGrab, FusionReader reader);

        // Handlers are created up front, they're not static
        public static void RegisterHandlersFromAssembly(Assembly targetAssembly)
        {
            if (targetAssembly == null) throw new NullReferenceException("Can't register from a null assembly!");

            FusionLogger.Log($"Populating GrabHandler list from {targetAssembly.GetName().Name}!");

            AssemblyUtilities.LoadAllValid<GrabGroupHandler>(targetAssembly, RegisterHandler);
        }

        public static void RegisterHandler<T>() where T : FusionMessageHandler => RegisterHandler(typeof(T));

        protected static void RegisterHandler(Type type)
        {
            GrabGroupHandler handler = Activator.CreateInstance(type) as GrabGroupHandler;

            if (handler.Group == null) {
                FusionLogger.Warn($"Didn't register {type.Name} because its grab group was null!");
            }
            else {
                byte index = (byte)handler.Group.Value;

                if (Handlers[index] != null) throw new Exception($"{type.Name} has the same index as {Handlers[index].GetType().Name}, we can't replace grab handlers!");

                FusionLogger.Log($"Registered {type.Name}");

                Handlers[index] = handler;
            }
        }

        public static void ReadGrab(ref SerializedGrab grab, FusionReader reader, GrabGroup group) {
            try {
                Handlers[(byte)group].HandleGrab(ref grab, reader);
            }
            catch (Exception e) {
                FusionLogger.Error($"Failed handling serialized grab with reason: {e.Message}\nTrace:{e.StackTrace}");
            }
        }


        public static readonly GrabGroupHandler[] Handlers = new GrabGroupHandler[byte.MaxValue];
    }
}
