#if MELONLOADER
using Il2CppInterop.Runtime.Attributes;
using Il2CppSLZ.Marrow.Warehouse;
using Il2CppTMPro;

using LabFusion.Menu;

using MelonLoader;

using UnityEngine;
using UnityEngine.UI;
#endif

namespace LabFusion.Marrow.Proxies
{
#if MELONLOADER
    [RegisterTypeInIl2Cpp]
#endif
    public class LevelRotationResultElement : MenuElement
    {
#if MELONLOADER
        public LevelRotationResultElement(IntPtr intPtr) : base(intPtr) { }

        public TMP_Text LevelRotationNameText { get; private set; } = null;

        public GameObject BorderGameObject { get; private set; } = null;

        public RawImage LevelIcon { get; private set; } = null;

        public Action OnSelectPressed, OnEditPressed;

        private bool _hasReferences = false;

        public void Awake()
        {
            GetReferences();
        }
        
        public void Highlight(bool highlighted)
        {
            GetReferences();

            BorderGameObject.SetActive(highlighted);
        }

        public void GetReferences()
        {
            if (_hasReferences) 
            { 
                return; 
            }

            // Level rotation name
            LevelRotationNameText = transform.Find("layout_TopOptions/levelRotationName_Background/text_LevelRotationName").GetComponent<TMP_Text>();

            // Icons
            LevelIcon = transform.Find("icon_Background/icon_Level").GetComponent<RawImage>();

            BorderGameObject = transform.Find("icon_Border").gameObject;

            _hasReferences = true;
        }

        [HideFromIl2Cpp]
        public void ApplyLevelIcon(string barcode)
        {
            GetReferences();

            if (barcode == null)
            {
                LevelIcon.texture = MenuResources.GetLevelIcon(MenuResources.EmptyIconTitle);
                return;
            }

            var levelCrate = CrateFilterer.GetCrate<LevelCrate>(new(barcode));

            if (levelCrate == null)
            {
                LevelIcon.texture = MenuResources.GetLevelIcon(MenuResources.EmptyIconTitle);
                return;
            }

            var title = levelCrate.Title;
            var modID = CrateFilterer.GetModID(levelCrate.Pallet);

            ElementIconHelper.SetLevelIcon(LevelIcon, title, modID);
        }

        public void Select()
        {
            OnSelectPressed?.Invoke();
        }

        public void Edit()
        {
            OnEditPressed?.Invoke();
        }
#else
        public void Select()
        {

        }

        public void Edit()
        {

        }
#endif
    }
}