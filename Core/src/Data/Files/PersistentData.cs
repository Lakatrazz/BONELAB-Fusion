using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using MelonLoader;

using LabFusion.Utilities;

namespace LabFusion.Data
{
    public static class PersistentData
    {
        public static string persistentPath { get; private set; }

        public static void OnPathInitialize() {
            string appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            persistentPath = appdata + ResourcePaths.AppDataSubFolder;

            FusionLogger.Log($"Data is at {appdata}", ConsoleColor.DarkCyan);
            ValidateDirectory(persistentPath);
        }

        public static void ValidateDirectory(string path) {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        public static string GetPath(string appended) => persistentPath + appended;
    }
}

