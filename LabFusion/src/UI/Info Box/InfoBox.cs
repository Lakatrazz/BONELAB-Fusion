using System;
using MelonLoader;
using UnityEngine;

namespace LabFusion.UI
{
    [RegisterTypeInIl2Cpp]
    public sealed class InfoBox : FusionUIMachine
    {
        public InfoBox(IntPtr intPtr) : base(intPtr) { }

        private InfoBoxPanelView _panelView;

        public InfoBoxPanelView PanelView => _panelView;

        protected override void AddPanelView(GameObject panel)
        {
            _panelView = panel.AddComponent<InfoBoxPanelView>();
        }

        protected override Transform GetGripRoot()
        {
            return transform.Find("Colliders");
        }
    }
}