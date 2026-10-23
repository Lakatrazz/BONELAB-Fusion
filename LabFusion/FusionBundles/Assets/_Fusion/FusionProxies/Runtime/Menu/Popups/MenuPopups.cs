#if MELONLOADER
using MelonLoader;
#endif

using UnityEngine;

namespace LabFusion.Marrow.Proxies
{
#if MELONLOADER
    [RegisterTypeInIl2Cpp]
#endif
    public class MenuPopups : MonoBehaviour
    {
#if MELONLOADER
        public MenuPopups(IntPtr intPtr) : base(intPtr) { }

        private MenuToolbar _toolbar = null;
        private Keyboard _keyboard = null;

        public MenuToolbar Toolbar => _toolbar;

        public Keyboard Keyboard => _keyboard;

        private void Awake()
        {
            _toolbar = GetComponentInChildren<MenuToolbar>(true);
            _keyboard = GetComponentInChildren<Keyboard>(true);

            _keyboard.OnOpen += OnKeyboardOpen;
            _keyboard.OnClose += OnKeyboardClose;
        }

        private void OnEnable()
        {
            _keyboard.Close();
        }

        private void OnKeyboardOpen()
        {
            _keyboard.gameObject.SetActive(true);
            _toolbar.gameObject.SetActive(false);
        }

        private void OnKeyboardClose()
        {
            _keyboard.gameObject.SetActive(false);
            _toolbar.gameObject.SetActive(true);
        }
#endif
    }
}