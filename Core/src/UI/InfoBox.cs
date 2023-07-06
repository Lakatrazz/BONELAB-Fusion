using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

using MelonLoader;

using UnityEngine.UI;

using SLZ.Interaction;

using LabFusion.Data;
using LabFusion.Utilities;

using SLZ.Marrow.Data;

using UnhollowerBaseLib;

namespace LabFusion.UI
{
    [RegisterTypeInIl2Cpp]
    public sealed class InfoBox : FusionUIMachine {
        public InfoBox(IntPtr intPtr) : base(intPtr) { }

        private InfoBoxPanelView _panelView;

        public InfoBoxPanelView PanelView => _panelView;

        protected override void AddPanelView(GameObject panel) {
            _panelView = panel.AddComponent<InfoBoxPanelView>();
        }

        protected override Transform GetGripRoot() {
            return transform.Find("Colliders");
        }
    }
}