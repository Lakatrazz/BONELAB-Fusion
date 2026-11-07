#if MELONLOADER
using MelonLoader;
#endif

namespace LabFusion.Marrow.Proxies
{
#if MELONLOADER
    [RegisterTypeInIl2Cpp]
#endif
    public class ButtonElement : LabelElement
    {
#if MELONLOADER
        public ButtonElement(IntPtr intPtr) : base(intPtr) { }

        private bool _interactable = true;

        public bool Interactable
        {
            get
            {
                return _interactable;
            }
            set
            {
                _interactable = value;

                Draw();
            }
        }
#endif
    }
}