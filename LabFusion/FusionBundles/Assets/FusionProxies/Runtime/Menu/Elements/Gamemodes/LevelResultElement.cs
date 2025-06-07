#if MELONLOADER
using Il2CppInterop.Runtime.Attributes;
using Il2CppSLZ.Marrow.Warehouse;
using Il2CppTMPro;

using LabFusion.Menu;

using MelonLoader;

using UnityEngine.UI;
#endif

namespace LabFusion.Marrow.Proxies
{
#if MELONLOADER
    [RegisterTypeInIl2Cpp]
#endif
    public class LevelResultElement : MenuElement
    {
#if MELONLOADER
        public LevelResultElement(IntPtr intPtr) : base(intPtr) { }

        public TMP_Text LevelNameText { get; private set; } = null;

        public RawImage LevelIcon { get; private set; } = null;

        public Action OnRemovePressed;

        private bool _hasReferences = false;

        public void Awake()
        {
            GetReferences();
        }
        
        public void GetReferences()
        {
            if (_hasReferences) 
            { 
                return; 
            }

            // Level name
            LevelNameText = transform.Find("layout_TopOptions/levelName_Background/text_LevelName").GetComponent<TMP_Text>();

            // Icons
            LevelIcon = transform.Find("icon_Background/icon_Level").GetComponent<RawImage>();

            _hasReferences = true;
        }

        public void Remove()
        {
            OnRemovePressed?.Invoke();
        }

        [HideFromIl2Cpp]
        public void ApplyLevel(string barcode)
        {
            GetReferences();

            var levelCrate = CrateFilterer.GetCrate<LevelCrate>(new(barcode));

            if (levelCrate == null)
            {
                LevelNameText.text = "No Level";
                LevelIcon.texture = MenuResources.GetLevelIcon(MenuResources.EmptyIconTitle);
                return;
            }

            var title = levelCrate.Title;
            LevelNameText.text = title;

            var modID = CrateFilterer.GetModID(levelCrate.Pallet);

            ElementIconHelper.SetLevelIcon(LevelIcon, title, modID);
        }
#else
        public void Remove()
        {

        }
#endif
    }
}