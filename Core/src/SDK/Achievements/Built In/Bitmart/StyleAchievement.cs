using LabFusion.SDK.Points;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.SDK.Achievements
{
    public abstract class StyleAchievement : Achievement {
        protected override void OnRegister() {
            CheckTasks();
            PointItemManager.OnItemUnlocked += OnItemUnlocked;
        }

        protected override void OnUnregister() {
            PointItemManager.OnItemUnlocked -= OnItemUnlocked;
        }

        private void OnItemUnlocked(PointItem item) {
            CheckTasks();
        }

        private void CheckTasks() {
            // Don't bother checking if it's already completed
            if (IsComplete)
                return;

            var unlockedItems = PointItemManager.GetUnlockedItems().Count;
            int debt = unlockedItems - CompletedTasks;
            if (debt <= 0)
                return;

            for (var i = 0; i < debt; i++) {
                IncrementTask();
            }
        }
    }
}
