#if MELONLOADER
using MelonLoader;
#endif

namespace LabFusion.Marrow.Proxies
{
#if MELONLOADER
    [RegisterTypeInIl2Cpp]
#endif
    public class FunctionElement : MenuElement
    {
#if MELONLOADER
        public Action OnPressed;

        public FunctionElement(IntPtr intPtr) : base(intPtr) { }

        public void Press()
        {
            OnPressed?.Invoke();
        }
#else
        public void Press()
        {

        }
#endif
    }
}