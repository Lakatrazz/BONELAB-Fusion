#if MELONLOADER
using MelonLoader;
#endif

namespace LabFusion.Marrow.Proxies
{
#if MELONLOADER
    [RegisterTypeInIl2Cpp]
#endif
    public class FunctionButton : MenuButton
    {
#if MELONLOADER
        public Action OnPressed;

        public FunctionButton(IntPtr intPtr) : base(intPtr) { }

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