using UnityEngine;

using MelonLoader;

using LabFusion.UI;

namespace LabFusion.SDK.Points
{
    [RegisterTypeInIl2Cpp]
    public sealed class PointShop : FusionUIMachine
    {
        public PointShop(IntPtr intPtr) : base(intPtr) { }

        private PointShopPanelView _panelView;

        public PointShopPanelView PanelView => _panelView;

        protected override void AddPanelView(GameObject panel)
        {
            _panelView = panel.AddComponent<PointShopPanelView>();
        }

        protected override Transform GetGripRoot()
        {
            return transform.Find("Art");
        }
    }
}