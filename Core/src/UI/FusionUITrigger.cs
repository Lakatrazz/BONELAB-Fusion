using LabFusion.Data;
using LabFusion.Utilities;

using MelonLoader;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.UI;

using SLZ.Interaction;

namespace LabFusion.UI
{
    [RegisterTypeInIl2Cpp]
    public sealed class FusionUITrigger : MonoBehaviour
    {
        public const float UIClickVolume = 0.4f;

        public static FusionUITrigger Selected { get; private set; }

        public FusionUITrigger(IntPtr intPtr) : base(intPtr) { }

        public Button button;

        private Hand _selectedHand = null;
        private bool _isActive = false;

        private void Awake()
        {
            var boxCollider = GetComponent<BoxCollider>();
            if (boxCollider != null)
            {
                var size = boxCollider.size;
                size.z *= 4.3f; // Multiplied in code incase this gets replaced with SLZ's UI
                boxCollider.size = size;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_isActive)
                return;

            var hand = Hand.Cache.Get(other.gameObject);

            if (hand != null && hand.manager == RigData.RigReferences.RigManager)
            {
                // Deselect active
                if (Selected != null && Selected != this)
                    Selected.Deselect();

                Select(hand);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (!_isActive)
                return;

            if (other.gameObject == _selectedHand.gameObject)
            {
                Deselect();
            }
        }

        private void OnDisable()
        {
            if (_isActive)
                Deselect();
        }

        private void Update()
        {
            if (_isActive)
            {
                var controller = _selectedHand.Controller;

                if (controller.GetPrimaryInteractionButtonDown())
                {
                    Click();
                }
            }
        }

        private void Click()
        {
            button.OnSubmit(null);

            FusionAudio.Play3D(transform.position, FusionContentLoader.UIConfirm, UIClickVolume);
        }

        private void Select(Hand hand)
        {
            _selectedHand = hand;
            _isActive = true;
            Selected = this;
            button.OnSelect(null);
            button.StartColorTween(button.colors.selectedColor, false);

            FusionAudio.Play3D(transform.position, FusionContentLoader.UISelect, UIClickVolume);
        }

        private void Deselect()
        {
            _selectedHand = null;
            _isActive = false;
            Selected = null;

            button.OnDeselect(null);
            button.StartColorTween(button.colors.normalColor, false);
        }
    }
}
