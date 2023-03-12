using LabFusion.Extensions;
using LabFusion.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.SDK.Gamemodes {
    public class GamemodePlaylist {
        public Gamemode gamemode;

        public float volume;
        public AudioSource source;
        public AudioClip[] clips;

        private int _currentClip = -1;
        private bool _playing = false;
        private bool _wasEnabled = true;

        public bool IsPlaying => _playing;

        public GamemodePlaylist(Gamemode gamemode, float volume, params AudioClip[] clips) {
            // Create the audio source
            var go = new GameObject("Gamemode Music") {
                hideFlags = HideFlags.DontUnloadUnusedAsset
            };
            source = go.AddComponent<AudioSource>();

            // Setup audio settings
            source.spatialBlend = 0f;
            source.volume = volume;

            PersistentAssetCreator.HookOnMusicMixerLoaded((m) => {
                if (source != null)
                    source.outputAudioMixerGroup = m;
            });

            // Store the clips
            clips.Shuffle();
            this.clips = clips;

            this.volume = volume;
            this.gamemode = gamemode;
        }

        public void Play() {
            _playing = true;

            if (!source.IsNOC()) {
                _currentClip = 0;
                SetClip(_currentClip);
                source.Play();
            }
        }

        private void SetClip(int index) {
            if (clips.Length > index)
                source.clip = clips[index];
        }

        private void NextClip() {
            _currentClip++;

            if (_currentClip >= clips.Length)
                _currentClip = 0;
        }

        public void Update() {
            if (!source.IsNOC() && _playing) {
                // Update volume
                if (gamemode.MusicEnabled != _wasEnabled) {
                    source.volume = gamemode.MusicEnabled ? volume : 0f;
                }

                _wasEnabled = gamemode.MusicEnabled;

                // Change song?
                if (!source.isPlaying) {
                    NextClip();
                    SetClip(_currentClip);

                    source.Play();
                }
            }
        }

        public void Stop() {
            _playing = false;

            if (!source.IsNOC()) {
                _currentClip = -1;
                source.Stop();
            }

            clips.Shuffle();
        }

        public void Dispose() {
            if (!source.IsNOC()) {
                GameObject.Destroy(source.gameObject);
            }
        }
    }
}
