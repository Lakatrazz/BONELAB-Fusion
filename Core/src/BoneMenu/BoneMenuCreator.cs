using BoneLib.BoneMenu.Elements;
using LabFusion.Extensions;
using LabFusion.Network;
using LabFusion.Preferences;
using LabFusion.Representation;
using LabFusion.Senders;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using UnityEngine;

namespace LabFusion.BoneMenu
{
    internal static partial class BoneMenuCreator
    {
        #region REMOVAL
        public static void RemoveEmptyCategory(MenuCategory parent, MenuCategory child) {
            if (child.Elements.Count <= 0)
                parent.Elements.RemoveInstance(child);
        }

        public static void RemoveEmptySubPanel(MenuCategory parent, SubPanelElement child)
        {
            if (child.Elements.Count <= 0)
                parent.Elements.RemoveInstance(child);
        }
        #endregion

        #region MENU CATEGORIES
        public static void CreateColorPreference(MenuCategory category, IFusionPref<Color> pref)
        {
            var currentColor = pref;
            var colorR = category.CreateFloatElement("Red", Color.red, currentColor.GetValue().r, 0.05f, 0f, 1f, (r) => {
                var color = currentColor.GetValue();
                color.r = r;
                currentColor.SetValue(color);
            });
            var colorG = category.CreateFloatElement("Green", Color.green, currentColor.GetValue().g, 0.05f, 0f, 1f, (g) => {
                var color = currentColor.GetValue();
                color.g = g;
                currentColor.SetValue(color);
            });
            var colorB = category.CreateFloatElement("Blue", Color.blue, currentColor.GetValue().b, 0.05f, 0f, 1f, (b) => {
                var color = currentColor.GetValue();
                color.b = b;
                currentColor.SetValue(color);
            });
            var colorPreview = category.CreateFunctionElement("■■■■■■■■■■■", currentColor.GetValue(), null);

            currentColor.OnValueChanged += (color) => {
                colorR.SetValue(color.r);
                colorG.SetValue(color.g);
                colorB.SetValue(color.b);
                colorPreview.SetColor(color);
            };
        }

        public static void CreateBytePreference(MenuCategory category, string name, byte increment, byte minValue, byte maxValue, IFusionPref<byte> pref)
        {
            var element = category.CreateIntElement(name, Color.white, pref.GetValue(), increment, minValue, maxValue, (v) => {
                pref.SetValue((byte)v);
            });

            pref.OnValueChanged += (v) => {
                element.SetValue(v);
            };
        }

        public static void CreateFloatPreference(MenuCategory category, string name, float increment, float minValue, float maxValue, IFusionPref<float> pref)
        {
            var element = category.CreateFloatElement(name, Color.white, pref.GetValue(), increment, minValue, maxValue, (v) => {
                pref.SetValue(v);
            });

            pref.OnValueChanged += (v) => {
                element.SetValue(v);
            };
        }

        public static void CreateBoolPreference(MenuCategory category, string name, IFusionPref<bool> pref)
        {
            var element = category.CreateBoolElement(name, Color.white, pref.GetValue(), (v) => {
                pref.SetValue(v);
            });

            pref.OnValueChanged += (v) => {
                element.SetValue(v);
            };
        }

        public static void CreateEnumPreference<TEnum>(MenuCategory category, string name, IFusionPref<TEnum> pref) where TEnum : Enum {
            var element = category.CreateEnumElement(name, Color.white, pref.GetValue(), (v) => {
                pref.SetValue(v);
            });

            pref.OnValueChanged += (v) => {
                element.SetValue(v);
            };
        }

        public static void CreateStringPreference(MenuCategory category, string name, IFusionPref<string> pref, Action<string> onValueChanged = null, int maxLength = PlayerIdManager.MaxNameLength) {
            string currentValue = pref.GetValue();
            var display = category.CreateFunctionElement(string.IsNullOrWhiteSpace(currentValue) ? $"No {name}" : $"{name}: {currentValue}", Color.white, null);
            var pasteButton = category.CreateFunctionElement($"Paste {name}", Color.white, () => {
                if (!Clipboard.ContainsText())
                    return;

                var text = Clipboard.GetText();
                text = text.LimitLength(maxLength);
                pref.SetValue(text);
            });
            var resetButton = category.CreateFunctionElement($"Reset {name}", Color.white, () => {
                pref.SetValue("");
            });

            pref.OnValueChanged += (v) => {
                display.SetName(string.IsNullOrWhiteSpace(v) ? $"No {name}" : $"{name}: {v}");

                onValueChanged?.Invoke(v);
            };
        }
        #endregion

        #region SUB PANEL
        public static void CreateColorPreference(SubPanelElement subPanel, IFusionPref<Color> pref)
        {
            var currentColor = pref;
            var colorR = subPanel.CreateFloatElement("Red", Color.red, currentColor.GetValue().r, 0.05f, 0f, 1f, (r) => {
                var color = currentColor.GetValue();
                color.r = r;
                currentColor.SetValue(color);
            });
            var colorG = subPanel.CreateFloatElement("Green", Color.green, currentColor.GetValue().g, 0.05f, 0f, 1f, (g) => {
                var color = currentColor.GetValue();
                color.g = g;
                currentColor.SetValue(color);
            });
            var colorB = subPanel.CreateFloatElement("Blue", Color.blue, currentColor.GetValue().b, 0.05f, 0f, 1f, (b) => {
                var color = currentColor.GetValue();
                color.b = b;
                currentColor.SetValue(color);
            });
            var colorPreview = subPanel.CreateFunctionElement("■■■■■■■■■■■", currentColor.GetValue(), null);

            currentColor.OnValueChanged += (color) => {
                colorR.SetValue(color.r);
                colorG.SetValue(color.g);
                colorB.SetValue(color.b);
                colorPreview.SetColor(color);
            };
        }

        public static void CreateBytePreference(SubPanelElement subPanel, string name, byte increment, byte minValue, byte maxValue, IFusionPref<byte> pref)
        {
            var element = subPanel.CreateIntElement(name, Color.white, pref.GetValue(), increment, minValue, maxValue, (v) => {
                pref.SetValue((byte)v);
            });

            pref.OnValueChanged += (v) => {
                element.SetValue(v);
            };
        }

        public static void CreateFloatPreference(SubPanelElement subPanel, string name, float increment, float minValue, float maxValue, IFusionPref<float> pref)
        {
            var element = subPanel.CreateFloatElement(name, Color.white, pref.GetValue(), increment, minValue, maxValue, (v) => {
                pref.SetValue(v);
            });

            pref.OnValueChanged += (v) => {
                element.SetValue(v);
            };
        }

        public static void CreateBoolPreference(SubPanelElement subPanel, string name, IFusionPref<bool> pref)
        {
            var element = subPanel.CreateBoolElement(name, Color.white, pref.GetValue(), (v) => {
                pref.SetValue(v);
            });

            pref.OnValueChanged += (v) => {
                element.SetValue(v);
            };
        }

        public static void CreateEnumPreference<TEnum>(SubPanelElement subPanel, string name, IFusionPref<TEnum> pref) where TEnum : Enum
        {
            var element = subPanel.CreateEnumElement(name, Color.white, pref.GetValue(), (v) => {
                pref.SetValue(v);
            });

            pref.OnValueChanged += (v) => {
                element.SetValue(v);
            };
        }

        public static void CreateStringPreference(SubPanelElement subPanel, string name, IFusionPref<string> pref, Action<string> onValueChanged = null, int maxLength = PlayerIdManager.MaxNameLength)
        {
            string currentValue = pref.GetValue();
            var display = subPanel.CreateFunctionElement(string.IsNullOrWhiteSpace(currentValue) ? $"No {name}" : $"{name}: {currentValue}", Color.white, null);
            var pasteButton = subPanel.CreateFunctionElement($"Paste {name}", Color.white, () => {
                if (!Clipboard.ContainsText())
                    return;

                var text = Clipboard.GetText();
                text = text.LimitLength(maxLength);
                pref.SetValue(text);
            });
            var resetButton = subPanel.CreateFunctionElement($"Reset {name}", Color.white, () => {
                pref.SetValue("");
            });

            pref.OnValueChanged += (v) => {
                display.SetName(string.IsNullOrWhiteSpace(v) ? $"No {name}" : $"{name}: {v}");

                onValueChanged?.Invoke(v);
            };
        }
        #endregion
    }
}
