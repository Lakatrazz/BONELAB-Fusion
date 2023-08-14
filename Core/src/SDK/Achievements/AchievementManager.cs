using LabFusion.Data;
using LabFusion.Extensions;
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

                achievement.Register();
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

        public static void IncrementAchievements<T>() where T : Achievement {
            foreach (var achievement in GetAchievements<T>()) {
                achievement.IncrementTask();
            }
        }

        public static List<T> GetAchievements<T>() where T : Achievement {
            List<T> list = new();

            foreach (var found in Achievements) {
                if (found is T result) {
                    list.Add(result);
                }
            }


            return list;
        }

        public static bool IsCompleted() {
            return GetAchievementProgress() >= 1f;
        }

        public static float GetAchievementProgress() {
            int totalAchievements = 0;
            int completedAchievements = 0;

            foreach (var achievement in LoadedAchievements) {
                // Ignore redacted achievements
                if (achievement.Redacted)
                    continue;

                // Increment our numbers
                if (achievement.IsComplete)
                    completedAchievements++;

                totalAchievements++;
            }

            return Mathf.Clamp01((float)completedAchievements / (float)totalAchievements);
        }

        public static IReadOnlyList<Achievement> GetSortedAchievements() {
            var list = LoadedAchievements.OrderBy(a => a.IsComplete).ThenBy(a => a.BitReward).ToList();
            list.RemoveAll((a) => a.Redacted && !a.IsComplete);
            return list;
        }

        public static IReadOnlyList<Achievement> LoadedAchievements => Achievements;

        internal static readonly List<Achievement> Achievements = new();
        internal static readonly FusionDictionary<string, Achievement> AchievementLookup = new();
    }
}
