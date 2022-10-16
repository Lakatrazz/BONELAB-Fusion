using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

using LabFusion.Data;
using LabFusion.Network;
using LabFusion.Extensions;

namespace LabFusion.Utilities {
    public static class SyncUtilities {
        public enum SyncGroup : byte {
            UNKNOWN = 0,
            PLAYER_BODY = 1,
            PROP = 2,
            NPC = 3,
            STATIC = 4,
            WORLD_GRIP = 5,
        }

        // Handlers are created up front, they're not static
        public static void RegisterGrabTypeFromAssembly(Assembly targetAssembly)
        {
            if (targetAssembly == null) throw new NullReferenceException("Can't register from a null assembly!");

            FusionLogger.Log($"Populating GrabType list from {targetAssembly.GetName().Name}!");

            // I am aware LINQ is kinda gross but this is works!
            targetAssembly.GetTypes()
                .Where(type => typeof(SerializedGrab).IsAssignableFrom(type) && !type.IsAbstract)
                .ForEach(type => {
                    try
                    {
                        RegisterGrabType(type);
                    }
                    catch (Exception e)
                    {
                        FusionLogger.Error(e.Message);
                    }
                });
        }

        public static void RegisterGrabType<T>() where T : Type => RegisterGrabType(typeof(T));

        public static void RegisterGrabType(Type type)
        {
            var attribute = type.GetCustomAttribute(typeof(SerializedGrabGroup));
            if (attribute == null || !(attribute is SerializedGrabGroup)) {
                FusionLogger.Warn($"Didn't register {type.Name} because its grab group was null!");
            }
            else {
                SyncGroup group = ((SerializedGrabGroup)attribute).group;

                if (SerializedGrabTypes.ContainsKey(group)) throw new Exception($"{type.Name} has the same grab group as {SerializedGrabTypes[group].GetType().Name}, we can't replace grab types!");

                FusionLogger.Log($"Registered {type.Name}");

                SerializedGrabTypes.Add(group, type);
            }
        }


        public static readonly Dictionary<SyncGroup, Type> SerializedGrabTypes = new Dictionary<SyncGroup, Type>(byte.MaxValue);
    }
}
