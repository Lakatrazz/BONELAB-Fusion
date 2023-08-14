using LabFusion.SDK.Achievements;
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
    public sealed class InfoBoxPanelView : FusionPanelView {
        public InfoBoxPanelView(IntPtr intPtr) : base(intPtr) { }

        protected override Vector3 Bounds => new(0.64f, 0.64f, 0.1f);

        private TMP_Text _versionText;
        private TMP_Text _changelogText;

        private TMP_Text _credits01Text;
        private TMP_Text _credits02Text;
        private TMP_Text _credits03Text;

        private Transform _groupPatchNotes;
        private Transform _groupCredits;
        private Transform _groupMystery;

        private Button _patchNotesButton;
        private Button _creditsButton;
        private Button _mysteryButton;

        protected override void OnAwake() {
            // Setup the menu
            SetupButtons();

            // Load the first page
            LoadPage(_groupPatchNotes.gameObject);
        }

        protected override void OnSetupReferences() {
            _versionText = _canvas.Find("text_versionNumber").GetComponent<TMP_Text>();
            _versionText.text = $"v{FusionMod.Version}";

            _groupPatchNotes = _canvas.Find("group_patchNotes");
            _groupCredits = _canvas.Find("group_credits");
            _groupMystery = _canvas.Find("group_mystery");

            _changelogText = _groupPatchNotes.Find("button_changelogContents").GetComponentInChildren<TMP_Text>();
            _changelogText.text = FusionMod.Changelog;

            // Setup credits text
            _credits01Text = _groupCredits.Find("text_credits01").GetComponentInChildren<TMP_Text>();
            _credits02Text = _groupCredits.Find("text_credits02").GetComponentInChildren<TMP_Text>();
            _credits03Text = _groupCredits.Find("text_credits03").GetComponentInChildren<TMP_Text>();

            if (FusionMod.Credits != null) {
                _credits01Text.text = FusionMod.Credits[0];
                _credits02Text.text = FusionMod.Credits[1];
                _credits03Text.text = FusionMod.Credits[2];
            }

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
                // Unlock the peter achievement
                if (AchievementManager.TryGetAchievement<HelloThere>(out var achievement))
                    achievement.IncrementTask();

                LoadPage(_groupMystery.gameObject);
            });
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
