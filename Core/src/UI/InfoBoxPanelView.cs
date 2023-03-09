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
            SetupText();

            // Load the first page
            LoadPage(_groupPatchNotes.gameObject);
        }

        private void SetupReferences() {
            _canvas = transform.Find("CANVAS");

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
            _patchNotesButton.onClick.AddListener((UnityAction)(() => {
                LoadPage(_groupPatchNotes.gameObject);
            }));
            _creditsButton.onClick.AddListener((UnityAction)(() => {
                LoadPage(_groupCredits.gameObject);
            }));
            _mysteryButton.onClick.AddListener((UnityAction)(() => {
                LoadPage(_groupMystery.gameObject);
            }));

            // Add clicking events to every button
            foreach (var button in transform.GetComponentsInChildren<Button>(true))
            {
                var collider = button.GetComponentInChildren<Collider>(true);
                if (collider != null)
                {
                    var interactor = collider.gameObject.AddComponent<FusionUITrigger>();
                    interactor.button = button;
                }
            }
        }

        private void SetupText()
        {
            foreach (var text in gameObject.GetComponentsInChildren<TMP_Text>(true))
            {
                text.font = PersistentAssetCreator.Font;
            }
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
