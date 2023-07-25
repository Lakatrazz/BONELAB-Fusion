using LabFusion.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using UnityEngine;

namespace LabFusion.SDK.Achievements {
    public static class AchievementManager
    {
        public static void LoadAchievements(Assembly assembly)
        {
            if (assembly == null)
                throw new NullReferenceException("Tried loading achievements from a null assembly!");

            AssemblyUtilities.LoadAllValid<Achievement>(assembly, RegisterAchievement);
        }

        public static void RegisterAchievement<T>() where T : Achievement => RegisterAchievement(typeof(T));

        private static void RegisterAchievement(Type type)
        {
            var achievement = Activator.CreateInstance(type) as Achievement;

            if (AchievementLookup.ContainsKey(achievement.Barcode))
                throw new ArgumentException($"Achievement with barcode {achievement.Barcode} was already registered.");
            else
            {
                Achievements.Add(achievement);
                AchievementLookup.Add(achievement.Barcode, achievement);

                if (AchievementSaveManager.Pointers.TryGetValue(achievement.Barcode, out var pointer)) {
                    achievement.Unpack(XElement.Parse(pointer.data));
                }
            }
        }

        public static bool TryGetAchievement(string barcode, out Achievement achievement)
        {
            if (barcode == null)
            {
                achievement = null;
                return false;
            }

            return AchievementLookup.TryGetValue(barcode, out achievement);
        }

        public static bool TryGetAchievement<T>(out T achievement) where T : Achievement {
            foreach (var found in Achievements) {
                if (found is T result) {
                    achievement = result;
                    return true;
                }
            }
            
            achievement = null;
            return false;
        }

        public static float GetAchievementProgress() {
            int totalAchievements = LoadedAchievements.Count;
            int completedAchievements = 0;

            foreach (var achievement in LoadedAchievements) {
                if (achievement.IsComplete)
                    completedAchievements++;
            }

            return Mathf.Clamp01((float)completedAchievements / (float)totalAchievements);
        }

        public static IReadOnlyList<Achievement> GetSortedAchievements() {
            return LoadedAchievements.OrderBy(a => a.IsComplete).ThenBy(a => a.BitReward).ToList();
        }

        public static IReadOnlyList<Achievement> LoadedAchievements => Achievements;

        internal static readonly List<Achievement> Achievements = new();
        internal static readonly Dictionary<string, Achievement> AchievementLookup = new();
    }
}
