using UnityEditor;

using UltEvents;

using UnityEditor.UIElements;
using UnityEngine.UIElements;

using System;

namespace LabFusion.Marrow.Integration
{
    [CustomEditor(typeof(GamemodeDropperSettings))]
    public class GamemodeDropperSettingsEditor : Editor
    {
        private SerializedProperty _itemDropsProperty;
        private SerializedProperty _maxItemsProperty;

        public void OnEnable()
        {
            _itemDropsProperty = serializedObject.FindProperty(nameof(GamemodeDropperSettings.ItemDrops));
            _maxItemsProperty = serializedObject.FindProperty(nameof(GamemodeDropperSettings.MaxItems));
        }

        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();

            var dropperSettings = target as GamemodeDropperSettings;

            if (!dropperSettings.TryGetComponent<LifeCycleEvents>(out var lifeCycleEvent))
            {
                var warnBox = new HelpBox("If you want to change the dropper settings, please add" +
                    " a LifeCycleEvents to this GameObject!", HelpBoxMessageType.Warning);
                root.Add(warnBox);

                var addLifeCycleEventsButton = new Button(() =>
                {
                    Undo.AddComponent<LifeCycleEvents>(dropperSettings.gameObject);
                })
                {
                    text = "Add LifeCycleEvents"
                };
                root.Add(addLifeCycleEventsButton);

                return root;
            }

            var itemDrops = new PropertyField(_itemDropsProperty);
            root.Add(itemDrops);

            var normalizeProbabilitiesButton = new Button(() =>
            {
                NormalizeProbabilities(dropperSettings, lifeCycleEvent);
            })
            {
                text = "Normalize Probabilities",
            };
            root.Add(normalizeProbabilitiesButton);

            var maxItems = new PropertyField(_maxItemsProperty);
            root.Add(maxItems);

            var infoBox = new HelpBox("The LifeCycleEvents on this GameObject is used to inject variables for these settings." +
    " Make sure nothing else is using the LifeCycleEvents on this same GameObject.", HelpBoxMessageType.Info);
            root.Add(infoBox);

            root.RegisterCallback<SerializedPropertyChangeEvent>(evt =>
            {
                OverrideLifeCycleEvent(dropperSettings, lifeCycleEvent);
            }, TrickleDown.TrickleDown);

            return root;
        }

        private void NormalizeProbabilities(GamemodeDropperSettings dropperSettings, LifeCycleEvents lifeCycleEvent)
        {
            Undo.RecordObject(dropperSettings, "Normalize Probabilities");

            float totalProbability = 0f;

            foreach (var drop in dropperSettings.ItemDrops)
            {
                totalProbability += drop.Probability;
            }

            if (totalProbability <= 0f)
            {
                totalProbability = 100f;
            }

            for (var i = 0; i < dropperSettings.ItemDrops.Count; i++)
            {
                var drop = dropperSettings.ItemDrops[i];

                var probability = drop.Probability;
                var normalized = probability / totalProbability * 100f;

                drop.Probability = normalized;

                dropperSettings.ItemDrops[i] = drop;
            }

            OverrideLifeCycleEvent(dropperSettings, lifeCycleEvent);
        }

        private void OverrideLifeCycleEvent(GamemodeDropperSettings dropperSettings, LifeCycleEvents lifeCycleEvent)
        {
            var ultEvent = new UltEvent();

            ApplyToUltEvent(dropperSettings, ultEvent);

            lifeCycleEvent.AwakeEvent = ultEvent;
        }

        private void ApplyToUltEvent(GamemodeDropperSettings dropperSettings, UltEvent ultEvent)
        {
            ultEvent.Clear();

            Action<string, float> addItemDropAction = dropperSettings.AddItemDrop;
            Action<int> setMaxItemsAction = dropperSettings.SetMaxItems;

            foreach (var drop in dropperSettings.ItemDrops)
            {
                // Make sure the crate reference is valid
                if (drop.ItemCrateReference == null || !drop.ItemCrateReference.IsValid())
                {
                    continue;
                }

                // If an item has no probability it can never be spawned anyways
                if (drop.Probability <= 0f)
                {
                    continue;
                }

                var call = ultEvent.AddPersistentCall(addItemDropAction);

                call.PersistentArguments[0].String = drop.ItemCrateReference.Barcode.ID;
                call.PersistentArguments[1].Float = drop.Probability;
            }

            var setMaxItemsCall = ultEvent.AddPersistentCall(setMaxItemsAction);
            setMaxItemsCall.PersistentArguments[0].Int = dropperSettings.MaxItems;
        }
    }
}