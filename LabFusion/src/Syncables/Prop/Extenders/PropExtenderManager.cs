﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LabFusion.Utilities;

namespace LabFusion.Syncables
{
    public static class PropExtenderManager
    {
        public static void RegisterExtendersFromAssembly(Assembly targetAssembly)
        {
            if (targetAssembly == null) throw new NullReferenceException("Can't register from a null assembly!");

#if DEBUG
            FusionLogger.Log($"Populating PropExtender list from {targetAssembly.GetName().Name}!");
#endif

            AssemblyUtilities.LoadAllValid<IPropExtender>(targetAssembly, RegisterExtender);
        }

        public static void RegisterExtender<T>() where T : IPropExtender => RegisterExtender(typeof(T));

        private static void RegisterExtender(Type type)
        {
            if (ExtenderTypes.Contains(type))
                throw new ArgumentException($"Extender type {type.Name} was already registered.");

            ExtenderTypes[_lastExtenderIndex++] = type;

#if DEBUG
            FusionLogger.Log($"Registered {type.Name}");
#endif
        }

        public static IReadOnlyList<IPropExtender> GetPropExtenders(PropSyncable syncable)
        {
            var list = new List<IPropExtender>();

            for (var i = 0; i < _lastExtenderIndex; i++)
            {
                var type = ExtenderTypes[i];

                var instance = Activator.CreateInstance(type) as IPropExtender;

                if (instance.ValidateExtender(syncable))
                {
                    list.Add(instance);
                }
            }

            return list;
        }

        private static int _lastExtenderIndex;

        public static readonly Type[] ExtenderTypes = new Type[1024];
    }
}
