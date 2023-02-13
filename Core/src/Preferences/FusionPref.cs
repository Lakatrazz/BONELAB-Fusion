using MelonLoader;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LabFusion.Utilities;

namespace LabFusion.Preferences {
    public enum PrefUpdateMode {
        IGNORE = 0,
        SERVER_UPDATE = 1,
        CLIENT_UPDATE = 2,
        LOCAL_UPDATE = 3,
    }

    public interface IFusionPref<T> {
        Action<T> OnValueChanged { get; set; }

        void SetValue(T value);
        T GetValue();
    }

    public class ReadonlyFusionPrev<T> : IFusionPref<T> {
        private readonly T _value;

        public Action<T> OnValueChanged { get; set; }

        public ReadonlyFusionPrev(T value) {
            _value = value;
        }

        public void SetValue(T value) { }

        public T GetValue() => _value;
    }

    public class FusionPref<T> : IFusionPref<T> {
        private readonly MelonPreferences_Category _category;
        private readonly MelonPreferences_Entry<T> _entry;
        private readonly PrefUpdateMode _mode;

        public Action<T> OnValueChanged { get; set; }

        public FusionPref(MelonPreferences_Category category, string name, T defaultValue, PrefUpdateMode mode = PrefUpdateMode.IGNORE) {
            _category = category;
            _entry = category.CreateEntry<T>(name, defaultValue);
            _mode = mode;

            FusionPreferences.OnFusionPreferencesLoaded += OnPreferencesLoaded;
        }

        public void SetValue(T value) {
            _entry.Value = value;
            OnValueChanged?.Invoke(value);
            _category.SaveToFile(false);
            PushUpdate();
        }

        public void OnPreferencesLoaded() {
            OnValueChanged?.Invoke(GetValue());
            PushUpdate();
        }

        private void PushUpdate() {
            switch (_mode) {
                default:
                case PrefUpdateMode.IGNORE:
                    break;
                case PrefUpdateMode.SERVER_UPDATE:
                    FusionPreferences.SendServerSettings();
                    MultiplayerHooking.Internal_OnServerSettingsChanged();
                    break;
                case PrefUpdateMode.CLIENT_UPDATE:
                    FusionPreferences.SendClientSettings();
                    break;
                case PrefUpdateMode.LOCAL_UPDATE:
                    MultiplayerHooking.Internal_OnServerSettingsChanged();
                    break;
            }
        }

        public T GetValue() {
            return _entry.Value;
        }

        public static implicit operator T(FusionPref<T> pref) => pref.GetValue();
    }
}
