using LabFusion.Data;
using LabFusion.Debugging;
using LabFusion.SDK.Gamemodes;
using LabFusion.Utilities;

using SLZ.Bonelab;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.SDK.Points {
    public static class PointSaveManager {
        [Serializable]
        public class PointSaveData {
            public string[] _boughtItems = null;
            public string[] _enabledItems = null;
            public int _bitCount = 0;

            public static PointSaveData CreateCurrent() {
                var data = new PointSaveData
                {
                    _boughtItems = _unlockedItems.ToArray(),
                    _enabledItems = _equippedItems.ToArray(),
                    _bitCount = _totalBits,
                };
                return data;
            }
        }

        private const string _filePath = "point_shop.dat";
        private const string _backupPath = "point_shop.dat.bak";

        public static void WriteToFile() {
            DataSaver.WriteBinary(_filePath, PointSaveData.CreateCurrent());
        }

        public static void WriteBackup() {
            string filePath = PersistentData.GetPath(_filePath);
            string backupPath = PersistentData.GetPath(_backupPath);

            if (File.Exists(filePath))
                File.Copy(filePath, backupPath, true);
        }

        public static void ReadFromFile() {
            var data = DataSaver.ReadBinary<PointSaveData>(_filePath);

            if (data != null) {
                if (data._boughtItems != null)
                    _unlockedItems = data._boughtItems.ToList();

                if (data._enabledItems != null)
                    _equippedItems = data._enabledItems.ToList();

                _totalBits = data._bitCount;
            }
        }

        public static bool IsUnlocked(string barcode) {
#if DEBUG
            if (FusionDevMode.UnlockEverything)
#pragma warning disable CS0162 // Unreachable code detected
                return true;
#pragma warning restore CS0162 // Unreachable code detected
#endif

#pragma warning disable CS0162 // Unreachable code detected
            return _unlockedItems.Contains(barcode);
#pragma warning restore CS0162 // Unreachable code detected
        }

        public static bool IsEquipped(string barcode) {
            return _equippedItems.Contains(barcode);
        }

        public static void UnlockItem(string barcode) {
            if (!_unlockedItems.Contains(barcode))
                _unlockedItems.Add(barcode);

            WriteToFile();
        }

        public static void LockItem(string barcode) {
            _unlockedItems.Remove(barcode);

            WriteToFile();
        }

        public static void SetEquipped(string barcode, bool isEquipped) {
            if (isEquipped) {
                if (!_equippedItems.Contains(barcode))
                    _equippedItems.Add(barcode);
            }
            else
                _equippedItems.Remove(barcode);

            WriteToFile();
        }

        public static int GetBitCount() => _totalBits;

        public static void SetBitCount(int count) {
            _totalBits = Mathf.Max(0, count);
            WriteToFile();
        }

        private static List<string> _unlockedItems = new List<string>();
        private static List<string> _equippedItems = new List<string>();
        private static int _totalBits;
    }
}
