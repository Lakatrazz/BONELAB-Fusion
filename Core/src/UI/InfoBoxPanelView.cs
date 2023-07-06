using LabFusion.Utilities;
using MelonLoader;

using System;

using TMPro;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace LabFusion.UI
{
    [RegisterTypeInIl2Cpp]
    public sealed class InfoBoxPanelView : MonoBehaviour {
        public InfoBoxPanelView(IntPtr intPtr) : base(intPtr) { }

        private Transform _canvas;
        private Transform _uiPlane;

        private TMP_Text _versionText;
        private TMP_Text _changelogText;

        private Transform _groupPatchNotes;
        private Transform _groupCredits;
        private Transform _groupMystery;

        private Button _patchNotesButton;
        private Button _creditsButton;
        private Button _mysteryButton;

        private void Awake() {
            // Setup the menu
            SetupReferences();
            SetupButtons();
            UIMachineUtilities.OverrideFonts(transform);

            // Load the first page
            LoadPage(_groupPatchNotes.gameObject);

            // Disable until the trigger is entered
            _canvas.gameObject.SetActive(false);
        }

        private void SetupReferences() {
            _canvas = transform.Find("CANVAS");
            _uiPlane = _canvas.Find("UIPLANE");

            UIMachineUtilities.CreateLaserCursor(_canvas, _uiPlane, new Vector3(0.64f, 0.64f, 0.1f));

            _versionText = _canvas.Find("text_versionNumber").GetComponent<TMP_Text>();
            _versionText.text = $"v{FusionMod.Version}";

            _groupPatchNotes = _canvas.Find("group_patchNotes");
            _groupCredits = _canvas.Find("group_credits");
            _groupMystery = _canvas.Find("group_mystery");

            _changelogText = _groupPatchNotes.Find("button_changelogContents").GetComponentInChildren<TMP_Text>();
            _changelogText.text = FusionMod.Changelog;

            _patchNotesButton = _canvas.Find("button_patchNotes").GetComponent<Button>();
            _creditsButton = _canvas.Find("button_credits").GetComponent<Button>();
            _mysteryButton = _canvas.Find("button_mystery").GetComponent<Button>();
        }

        private void SetupButtons()
        {
            // Setup page buttons
            _patchNotesButton.AddClickEvent(() => {
                LoadPage(_groupPatchNotes.gameObject);
            });
            _creditsButton.AddClickEvent(() => {
                LoadPage(_groupCredits.gameObject);
            });
            _mysteryButton.AddClickEvent(() => {
                LoadPage(_groupMystery.gameObject);
            });

            // Add clicking events to every button
            UIMachineUtilities.AddButtonTriggers(transform);
        }

        private void LoadPage(GameObject page) {
            // Disable all other pages
            _groupPatchNotes.gameObject.SetActive(false);
            _groupCredits.gameObject.SetActive(false);
            _groupMystery.gameObject.SetActive(false);

            // Enable the target page
            page.SetActive(true);
        }
    }
}
