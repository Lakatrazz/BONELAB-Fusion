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

        public static void OnPathInitialize() {
            string appData = Application.persistentDataPath;
            PersistentPath = appData + ResourcePaths.AppDataSubFolder;

            FusionLogger.Log($"Data is at {PersistentPath}", ConsoleColor.DarkCyan);
            ValidateDirectory(PersistentPath);
        }

        public static void ValidateDirectory(string path) {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        public static string GetPath(string appended) => PersistentPath + appended;
    }
}

