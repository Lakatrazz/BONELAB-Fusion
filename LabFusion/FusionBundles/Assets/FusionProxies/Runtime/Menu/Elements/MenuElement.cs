using UnityEngine;

#if MELONLOADER
using MelonLoader;
#endif

namespace LabFusion.Marrow.Proxies
{
#if MELONLOADER
    [RegisterTypeInIl2Cpp]
#endif
    public class MenuElement : MonoBehaviour
    {
#if MELONLOADER
        public MenuElement(IntPtr intPtr) : base(intPtr) { }

        private string _title = "Element";

        public string Title
        {
            get
            {
                return _title;
            }
            set
            {
                _title = value;

                Draw();
            }
        }

        public event Action OnCleared;

        protected virtual void OnEnable()
        {
            Draw();
        }

        protected virtual void OnDestroy()
        {
            Clear();
        }

        public void Clear()
        {
            OnClearValues();

            OnCleared?.Invoke();
            OnCleared = null;
        }

        protected virtual void OnClearValues()
        {
            _title = "Element";
        }

        public void Draw()
        {
            OnDraw();
        }

        protected virtual void OnDraw() { }
#endif
    }
}