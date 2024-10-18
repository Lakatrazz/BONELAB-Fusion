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

        public event Action OnDestroyed;

        protected virtual void OnEnable()
        {
            Draw();
        }

        protected virtual void OnDestroy()
        {
            OnDestroyed?.Invoke();
            OnDestroyed = null;
        }

        public void Draw()
        {
            OnDraw();
        }

        protected virtual void OnDraw() { }
#endif
    }
}