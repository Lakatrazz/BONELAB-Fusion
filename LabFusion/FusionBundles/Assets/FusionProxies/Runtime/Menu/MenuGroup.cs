#if MELONLOADER
using MelonLoader;
#endif

using UnityEngine;
using UnityEngine.UI;

namespace LabFusion.Marrow.Proxies
{
#if MELONLOADER
    [RegisterTypeInIl2Cpp]
#endif
    public class MenuGroup : MenuButton
    {
#if MELONLOADER
        public MenuGroup(IntPtr intPtr) : base(intPtr) { }

        private List<MenuButton> _buttons = new();

        public List<MenuButton> Buttons => _buttons;

        private FunctionButton _functionTemplate;
        private StringButton _stringTemplate;
        private IntButton _intTemplate;
        private FloatButton _floatTemplate;
        private BoolButton _boolTemplate;
        private MenuGroup _groupTemplate;

        private bool _hasTemplates = false;

        protected override void Awake()
        {
            base.Awake();

            GetTemplates();
        }

        private void GetTemplates()
        {
            if (_hasTemplates)
            {
                return;
            }

            _functionTemplate = GetComponentInChildren<FunctionButton>(true);
            _stringTemplate = GetComponentInChildren<StringButton>(true);
            _intTemplate = GetComponentInChildren<IntButton>(true);
            _floatTemplate = GetComponentInChildren<FloatButton>(true);
            _boolTemplate = GetComponentInChildren<BoolButton>(true);

            foreach (var child in transform)
            {
                var childTransform = child.TryCast<Transform>();

                var childGroupTemplate = childTransform.GetComponent<MenuGroup>();

                if (childGroupTemplate != null)
                {
                    _groupTemplate = childGroupTemplate;
                    break;
                }
            }

            _hasTemplates = true;
        }

        protected virtual void OnElementAdded(MenuButton element) 
        {
            element.gameObject.SetActive(true);
        }

        public FunctionButton AddFunction(string title, Action action)
        {
            GetTemplates();

            if (_functionTemplate == null)
            {
                return null;
            }

            var newFunction = GameObject.Instantiate(_functionTemplate, transform, false);
            newFunction.name = title;
            newFunction.Title = title;

            newFunction.OnPressed += action;

            _buttons.Add(newFunction);

            OnElementAdded(newFunction);

            return newFunction;
        }

        public StringButton AddString(string title, string value, Action<string> onValueChanged = null)
        {
            GetTemplates();

            if (_stringTemplate == null)
            {
                return null;
            }

            var newString = GameObject.Instantiate(_stringTemplate, transform, false);
            newString.name = title;
            newString.Title = title;

            newString.Value = value;

            if (onValueChanged != null)
            {
                newString.OnValueChanged += onValueChanged;
            }

            _buttons.Add(newString);

            OnElementAdded(newString);

            return newString;
        }

        public IntButton AddInt(string title, int value, int increment, int minValue, int maxValue, Action<int> onValueChanged = null)
        {
            GetTemplates();

            if (_intTemplate == null)
            {
                return null;
            }

            var newInt = GameObject.Instantiate(_intTemplate, transform, false);
            newInt.name = title;
            newInt.Title = title;

            newInt.Value = value;
            newInt.Increment = increment;
            newInt.MinValue = minValue;
            newInt.MaxValue = maxValue;

            if (onValueChanged != null)
            {
                newInt.OnValueChanged += onValueChanged;
            }

            _buttons.Add(newInt);

            OnElementAdded(newInt);

            return newInt;
        }

        public FloatButton AddFloat(string title, float value, float increment, float minValue, float maxValue, Action<float> onValueChanged = null)
        {
            GetTemplates();

            if (_floatTemplate == null)
            {
                return null;
            }

            var newFloat = GameObject.Instantiate(_floatTemplate, transform, false);
            newFloat.name = title;
            newFloat.Title = title;

            newFloat.Value = value;
            newFloat.Increment = increment;
            newFloat.MinValue = minValue;
            newFloat.MaxValue = maxValue;

            if (onValueChanged != null)
            {
                newFloat.OnValueChanged += onValueChanged;
            }

            _buttons.Add(newFloat);

            OnElementAdded(newFloat);

            return newFloat;
        }

        public BoolButton AddBool(string title, bool value, Action<bool> onValueChanged = null)
        {
            GetTemplates();

            if (_boolTemplate == null)
            {
                return null;
            }

            var newBool = GameObject.Instantiate(_boolTemplate, transform, false);
            newBool.name = title;
            newBool.Title = title;

            newBool.Value = value;

            if (onValueChanged != null)
            {
                newBool.OnValueChanged += onValueChanged;
            }

            _buttons.Add(newBool);

            OnElementAdded(newBool);

            return newBool;
        }

        public MenuGroup AddGroup(string title)
        {
            GetTemplates();

            if (_groupTemplate == null)
            {
                return null;
            }

            var newGroup = GameObject.Instantiate(_groupTemplate, transform, false);
            newGroup.name = title;
            newGroup.Title = title;

            _buttons.Add(newGroup);

            OnElementAdded(newGroup);

            return newGroup;
        }
#endif
    }
}