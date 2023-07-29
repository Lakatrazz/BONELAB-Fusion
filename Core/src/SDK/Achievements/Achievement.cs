using LabFusion.Data;
using LabFusion.Extensions;
using LabFusion.SDK.Points;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using UnityEngine;

namespace LabFusion.SDK.Achievements
{
    public abstract class Achievement : IXMLPackable {
        public static event Action<Achievement> OnAchievementUpdated;
        public static event Action<Achievement> OnAchievementCompleted;

        // The title of the achievement
        public abstract string Title { get; }

        // The description of the achievement
        public abstract string Description { get; }

        // The amount of bits to be rewarded upon completion.
        public abstract int BitReward { get; }

        // The text representing progress.
        public virtual string Progress => $"{CompletedTasks} out of {MaxTasks} Tasks Completed";

        // The barcode pointing to the achievement
        public virtual string Barcode => $"{Title}.Achievement";

        // The preview image of the achievement in the menu. (Optional)
        public virtual Texture2D PreviewImage => null;

        // Whether or not the achievement has been completed.
        public bool IsComplete => CompletedTasks >= MaxTasks;

        // If true, this achievement will not show in the menu or count towards progress.
        public virtual bool Redacted => false;

        // The amount of tasks currently completed.
        public int CompletedTasks { get; protected set; }

        // The total number of tasks required.
        public virtual int MaxTasks => 1;

        public void Register() {
            OnRegister();
        }

        protected virtual void OnRegister() { }

        public void Unregister() {
            OnUnregister();
        }

        protected virtual void OnUnregister() { }

        public virtual void Pack(XElement element) {
            element.SetAttributeValue(nameof(CompletedTasks), CompletedTasks);
        }

        public virtual void Unpack(XElement element) {
            if (element.TryGetAttribute(nameof(CompletedTasks), out var rawTasks) && int.TryParse(rawTasks, out var number))
                CompletedTasks = number;
        }

        public virtual void IncrementTask() {
            if (IsComplete)
                return;

            CompletedTasks++;

            if (IsComplete)
                Complete();

            OnAchievementUpdated?.Invoke(this);
        }

        private void Complete() {
            PointItemManager.RewardBits(BitReward);
            OnAchievementCompleted?.Invoke(this);

            OnComplete();
        }

        protected virtual void OnComplete() { }
    }
}
