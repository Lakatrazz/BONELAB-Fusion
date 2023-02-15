using LabFusion.Data;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LabFusion.Points {
    public static class PointSaveManager {
        [Serializable]
        public class PointSaveData {
            public string[] _boughtItems;
            public int _bitCount;

            public PointSaveData() {
                _boughtItems = _unlockedItems.ToArray();
                _bitCount = _totalBits;
            }
        }

        private const string _filePath = "point_shop.dat";

        public static void WriteToFile() {
            DataSaver.WriteBinary(_filePath, new PointSaveData());
        }

        public static void ReadFromFile() {
            var data = DataSaver.ReadBinary<PointSaveData>(_filePath);

            if (data != null) {
                _unlockedItems = data._boughtItems.ToList();
                _totalBits = data._bitCount;
            }
        }

        public static bool IsUnlocked(string barcode) {
            return _unlockedItems.Contains(barcode);
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

        public static int GetBitCount() => _totalBits;

        public static void SetBitCount(int count) {
            _totalBits = Mathf.Max(0, count);
            WriteToFile();
        }

        private static List<string> _unlockedItems = new List<string>();
        private static int _totalBits;
    }
}
