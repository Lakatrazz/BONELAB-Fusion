using LabFusion.SDK.Achievements;
using LabFusion.Utilities;

using MelonLoader;
using Il2CppTMPro;
using Il2CppInterop.Runtime.Attributes;
using UnityEngine;
using UnityEngine.UI;

namespace LabFusion.UI
{
    [RegisterTypeInIl2Cpp]
    public sealed class CupBoardPanelView : FusionPanelView
    {
        public CupBoardPanelView(IntPtr intPtr) : base(intPtr) { }

        protected override Vector3 Bounds => new(2.14f, 1.4f, 0.1f);

        private Transform _achievementButtonsRoot;
        private Transform _arrowButtonsRoot;

        private Slider _progressSlider;
        private TMP_Text _progressText;

        private Transform[] _achievementPanels;
        private int _achievementButtonCount;

        private int _currentPageIndex = 0;
        private int _pageCount;
        private Texture _defaultPreview;

        private TMP_Text _pageCountText;

        public int PageCount => _pageCount;

        [HideFromIl2Cpp]
        public IReadOnlyList<Achievement> PageItems => AchievementManager.GetSortedAchievements();

        protected override void OnAwake()
        {
            base.OnAwake();
            SetupArrows();

            Achievement.OnAchievementUpdated += OnAchievementUpdated;

            LoadPage();
        }

        private void OnDestroy()
        {
            Achievement.OnAchievementUpdated -= OnAchievementUpdated;
        }

        [HideFromIl2Cpp]
        private void OnAchievementUpdated(Achievement achievement)
        {
            LoadPage();
        }

        protected override void OnSetupReferences()
        {
            _achievementButtonsRoot = _canvas.Find("achievement_Buttons");
            _arrowButtonsRoot = _canvas.Find("arrow_Buttons");

            _progressSlider = _canvas.Find("bar_Progress").GetComponent<Slider>();
            _progressText = _progressSlider.transform.Find("text").GetComponent<TMP_Text>();

            _pageCountText = _arrowButtonsRoot.Find("button_pageCount").GetComponentInChildren<TMP_Text>();

            _achievementButtonCount = _achievementButtonsRoot.childCount;
            _achievementPanels = new Transform[_achievementButtonCount];
            for (var i = 0; i < _achievementButtonCount; i++)
            {
                _achievementPanels[i] = _achievementButtonsRoot.GetChild(i);

                if (i == 0)
                {
                    _defaultPreview = _achievementPanels[i].GetComponentInChildren<RawImage>().texture;
                }
            }
        }

        private void SetupArrows()
        {
            // Setup the arrows
            _arrowButtonsRoot.Find("button_lastPage").GetComponent<Button>().AddClickEvent(() =>
            {
                LastPage();
            });
            _arrowButtonsRoot.Find("button_nextPage").GetComponent<Button>().AddClickEvent(() =>
            {
                NextPage();
            });
        }

        public void NextPage()
        {
            _currentPageIndex++;

            if (_currentPageIndex >= PageCount)
                _currentPageIndex = PageCount - 1;

            LoadPage();
        }

        public void LastPage()
        {
            _currentPageIndex--;

            if (_currentPageIndex < 0)
                _currentPageIndex = 0;

            LoadPage();
        }

        private void LoadPage()
        {
            // Get page count
            if (PageItems.Count <= 0)
            {
                _pageCount = 0;
            }
            else
            {
                _pageCount = (int)Math.Ceiling((double)PageItems.Count / (double)_achievementButtonCount);
            }

            _currentPageIndex = ManagedMathf.Clamp(_currentPageIndex, 0, PageCount);

            _pageCountText.text = $"Page {_currentPageIndex + 1} out of {Math.Max(1, PageCount)}";

            // Loop through every panel
            for (var i = 0; i < _achievementButtonCount; i++)
            {
                var panel = _achievementPanels[i];
                var achievementIndex = GetAchievementIndex(i);

                if (PageItems.Count <= achievementIndex)
                {
                    panel.gameObject.SetActive(false);
                    continue;
                }

                panel.gameObject.SetActive(true);

                LoadAchievement(panel, PageItems[achievementIndex]);
            }

            LoadProgress();
        }

        private void LoadProgress()
        {
            float progress = AchievementManager.GetAchievementProgress();
            _progressSlider.Set(progress, false);
            _progressText.text = $"{progress * 100f}%";
        }

        [HideFromIl2Cpp]
        private void LoadAchievement(Transform transform, Achievement achievement)
        {
            TMP_Text title = transform.Find("title").GetComponent<TMP_Text>();
            TMP_Text description = transform.Find("description").GetComponent<TMP_Text>();
            TMP_Text progress = transform.Find("progress").GetComponent<TMP_Text>();
            RawImage icon = transform.Find("icon").GetComponent<RawImage>();
            GameObject checkmark = transform.Find("completed").gameObject;
            GameObject exclamation = transform.Find("incompleted").gameObject;
            TMP_Text bitCount = title.transform.Find("button_bitCount").Find("text").GetComponent<TMP_Text>();

            title.text = achievement.Title;
            description.text = achievement.Description;
            bitCount.text = achievement.BitReward.ToString();
            progress.text = achievement.Progress;

            if (achievement.PreviewImage != null)
            {
                icon.texture = achievement.PreviewImage;
            }
            else
            {
                icon.texture = _defaultPreview;
            }

            checkmark.SetActive(achievement.IsComplete);
            exclamation.SetActive(!achievement.IsComplete);
        }

        private int GetAchievementIndex(int index)
        {
            return index + (_currentPageIndex * _achievementButtonCount);
        }
    }
}
