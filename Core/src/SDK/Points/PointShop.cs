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

using LabFusion.UI;

namespace LabFusion.SDK.Points
{
    [RegisterTypeInIl2Cpp]
    public sealed class PointShop : FusionUIMachine {
        public PointShop(IntPtr intPtr) : base(intPtr) { }

        private PointShopPanelView _panelView;

        public PointShopPanelView PanelView => _panelView;

        protected override void OnAwake() {
            // Setup audio
            PersistentAssetCreator.HookOnSFXMixerLoaded((m) => {
                if (gameObject != null) {
                    AudioSource[] sources = gameObject.GetComponentsInChildren<AudioSource>(true);

                    foreach (var source in sources) {
                        source.outputAudioMixerGroup = m;
                    }
                }
            });
        }

        protected override void AddPanelView(GameObject panel) {
            _panelView = panel.AddComponent<PointShopPanelView>();
        }

        protected override Transform GetGripRoot()
        {
            return transform.Find("Art");
        }
    }
}