using UnityEngine;

using MelonLoader;
using LabFusion.Utilities;

using LabFusion.SDK.Achievements;
using Il2CppInterop.Runtime.Attributes;

namespace LabFusion.UI
{
    [RegisterTypeInIl2Cpp]
    public sealed class CupBoard : FusionUIMachine
    {
        private const float _maxMusicVolume = 0.3f;

        public CupBoard(IntPtr intPtr) : base(intPtr) { }

        private CupBoardPanelView _panelView;

        public CupBoardPanelView PanelView => _panelView;

        private AudioSource[] _musicLayers = null;
        private int _layerCount = 0;

        private GameObject _completionistGo = null;

        protected override void OnAwake()
        {
            base.OnAwake();

            _musicLayers = transform.Find("SFX/Music").GetComponentsInChildren<AudioSource>();
            _layerCount = _musicLayers.Length;

            _completionistGo = transform.Find("Art/Offset/Completionist").gameObject;

            Achievement.OnAchievementCompleted += OnAchievementCompleted;
            LoadProgress();
        }

        private void OnDestroy()
        {
            Achievement.OnAchievementCompleted -= OnAchievementCompleted;
        }

        [HideFromIl2Cpp]
        private void OnAchievementCompleted(Achievement achievement)
        {
            LoadProgress();
        }

        private void LoadProgress()
        {
            var progress = AchievementManager.GetAchievementProgress();
            LoadEffects(progress);
            LoadMusic(progress);
        }

        private void LoadEffects(float progress)
        {
            bool isComplete = progress >= 1f;
            _completionistGo.SetActive(isComplete);
        }

        private void LoadMusic(float progress)
        {
            // The first layer should always be max volume
            _musicLayers[0].volume = _maxMusicVolume;

            // The other layers will increase in volume as you gain more progress
            int offsetCount = _layerCount - 1;
            float scaled = progress * offsetCount;

            for (var i = 0; i < offsetCount; i++)
            {
                float value = scaled - i;
                _musicLayers[i + 1].volume = ManagedMathf.Lerp(0f, _maxMusicVolume, value);
            }
        }

        protected override void AddPanelView(GameObject panel)
        {
            _panelView = panel.AddComponent<CupBoardPanelView>();
        }

        protected override Transform GetGripRoot()
        {
            return transform.Find("Colliders");
        }
    }
}