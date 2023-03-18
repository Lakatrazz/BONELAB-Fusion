using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using MelonLoader;

using LabFusion.Utilities;

using BoneLib;

using UnityEngine;
using System.Runtime.InteropServices;

namespace LabFusion.Data
{
    public static class PersistentData
    {
        public static string PersistentPath { get; private set; }

        private static bool _initialized = false;

        public static void OnPathInitialize() {
            if (_initialized)
                return;

            string appData = Application.persistentDataPath;
            PersistentPath = appData + ResourcePaths.AppDataSubFolder;

            FusionLogger.Log($"Data is at {PersistentPath}.", ConsoleColor.DarkCyan);
            ValidateDirectory(PersistentPath);

            _initialized = true;
        }

        public static void ValidateDirectory(string path) {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        public static string GetPath(string appended) {
            if (!_initialized) {
#if DEBUG
                FusionLogger.Warn("Tried getting a persistent path before it was initialized!\n" +
                    "Forcing initialization!");
#endif

                OnPathInitialize();
            }

            return PersistentPath + appended; 
        }
    }
}

