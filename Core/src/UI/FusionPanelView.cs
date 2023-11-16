using LabFusion.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.UI
{
    public class FusionPanelView : MonoBehaviour
    {
        public FusionPanelView(IntPtr intPtr) : base(intPtr) { }

        protected virtual Vector3 Bounds => new(1f, 1f, 0.1f);

        protected Transform _canvas;
        protected Transform _uiPlane;

        private void Awake()
        {
            UIMachineUtilities.OverrideFonts(transform);
            UIMachineUtilities.AddButtonTriggers(transform);

            SetupReferences();

            OnAwake();

            _canvas.gameObject.SetActive(false);
        }

        private void SetupReferences()
        {
            _canvas = transform.Find("CANVAS");
            _uiPlane = _canvas.Find("UIPLANE");

            UIMachineUtilities.CreateLaserCursor(_canvas, _uiPlane, Bounds);

            OnSetupReferences();
        }

        protected virtual void OnAwake() { }

        protected virtual void OnSetupReferences() { }
    }
}
