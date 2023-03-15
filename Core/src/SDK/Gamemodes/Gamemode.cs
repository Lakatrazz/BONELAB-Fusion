using BoneLib.BoneMenu.Elements;

using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.Senders;
using LabFusion.Utilities;
using System;
using System.Collections.Generic;

using UnityEngine;

namespace LabFusion.SDK.Gamemodes {
    public abstract class Gamemode {
        public const float DefaultMusicVolume = 0.4f;

        internal static Gamemode _activeGamemode = null;
        public static Gamemode ActiveGamemode => _activeGamemode;

        public static bool MusicToggled { get; internal set; } = true;
        public static bool LateJoining { get; internal set; } = false;

        internal ushort? _tag = null;
        public ushort? Tag => _tag;

        // Gamemode info
        public bool IsStarted { get; private set; }

        // Gamemode settings
        public abstract string GamemodeCategory { get; }
        public abstract string GamemodeName { get; }

        public virtual bool VisibleInBonemenu { get; } = true;
        public virtual bool PreventNewJoins { get; } = false;
        public virtual bool AutoStopOnSceneLoad { get; } = true;
        public virtual bool AutoHolsterOnDeath { get; } = true;

        // Cheats
        public virtual bool DisableDevTools { get; } = false;
        public virtual bool DisableSpawnGun { get; } = false;
        public virtual bool DisableManualUnragdoll { get; } = false;

        private readonly Dictionary<string, string> _internalMetadata = new Dictionary<string, string>();
        public Dictionary<string, string> Metadata => _internalMetadata;

        // Music
        public virtual bool MusicEnabled => MusicToggled;
        public virtual bool ManualPlaylist { get; } = false;

        protected GamemodePlaylist _playlist = null;

        internal void GamemodeRegistered() {
            MultiplayerHooking.OnMainSceneInitialized += OnMainSceneInitialized;
            MultiplayerHooking.OnLoadingBegin += OnLoadingBegin;

            OnGamemodeRegistered();
        }

        internal void GamemodeUnregistered() {
            MultiplayerHooking.OnMainSceneInitialized -= OnMainSceneInitialized;
            MultiplayerHooking.OnLoadingBegin -= OnLoadingBegin;

            OnGamemodeUnregistered();
        }

        public void SetPlaylist(float volume = 1f, params AudioClip[] clips) {
            bool wasPlaying = false;

            if (_playlist != null) {
                wasPlaying = _playlist.IsPlaying;
                _playlist.Dispose();
                _playlist = null;
            }

            _playlist = new GamemodePlaylist(this, volume, clips);

            if (wasPlaying && !ManualPlaylist)
                PlayPlaylist();
        }

        public void PlayPlaylist() {
            if (_playlist != null)
                _playlist.Play();
        }

        public void StopPlaylist() {
            if (_playlist != null) {
                _playlist.Stop();
            }
        }

        public void DisposePlaylist() {
            if (_playlist != null) {
                _playlist.Dispose();
                _playlist = null;
            }
        }


        public void UpdatePlaylist() {
            if (_playlist != null)
                _playlist.Update();
        }

        public bool IsActive() => ActiveGamemode == this;

        public virtual void OnGamemodeRegistered() { }

        public virtual void OnGamemodeUnregistered() { }

        public virtual void OnMainSceneInitialized() { }

        public virtual void OnLoadingBegin() { }

        protected FunctionElement _gamemodeToggleElement = null;

        public virtual void OnBoneMenuCreated(MenuCategory category) {
            // Default elements
            _gamemodeToggleElement = category.CreateFunctionElement("Start Gamemode", Color.yellow, () => {
                if (!IsActive()) {
                    StartGamemode(true);
                }
                else {
                    StopGamemode();
                }
            });
        }

        public bool StartGamemode(bool forceStopCurrent = false) {
            // We can only start the gamemode as a server!
            if (!NetworkInfo.IsServer) {
                return false;
            }

            // Check if theres an already active gamemode
            if (ActiveGamemode != null) {
                // If we can't force stop, just return.
                if (!forceStopCurrent)
                    return false;

                // Otherwise, force stop the active gamemode
                ActiveGamemode.StopGamemode();
            }

            TrySetMetadata(GamemodeHelper.IsStartedKey, "True");
            return true;
        }

        public bool StopGamemode() {
            // We can only stop the gamemode as a server!
            if (!NetworkInfo.IsServer) {
                return false;
            }

            // Make sure the active gamemode is us
            if (!IsActive())
                return false;

            TrySetMetadata(GamemodeHelper.IsStartedKey, "False");
            return true;
        }

        internal void Internal_SetGamemodeState(bool isStarted) {
            // Make sure we aren't doing this twice
            if (IsStarted == isStarted)
                return;

            if (isStarted) {
                MultiplayerHooking.OnShouldAllowConnection += Internal_UserJoinCheck;

                GamemodeManager.Internal_SetActiveGamemode(this);
                IsStarted = true;
                OnStartGamemode();

                if (!ManualPlaylist)
                    PlayPlaylist();
            }
            else {
                MultiplayerHooking.OnShouldAllowConnection -= Internal_UserJoinCheck;

                GamemodeManager.Internal_SetActiveGamemode(null);
                IsStarted = false;
                OnStopGamemode();

                if (!ManualPlaylist)
                    StopPlaylist();
            }
        }

        private bool Internal_UserJoinCheck(ulong userId, out string reason) {
            if (ActiveGamemode == this && (PreventNewJoins || !LateJoining)) {
                reason = $"Gamemode {GamemodeName} is currently running!";
                return false;
            }

            reason = "";
            return true;
        }

        // Gamemode state
        protected virtual void OnStartGamemode() { 
            // Set default bonemenu element
            if (_gamemodeToggleElement != null) {
                _gamemodeToggleElement.SetName("Stop Gamemode");
            }
        }

        protected virtual void OnStopGamemode() {
            // Set default bonemenu element
            if (_gamemodeToggleElement != null) {
                _gamemodeToggleElement.SetName("Start Gamemode");
            }
        }

        // Update methods
        public void FixedUpdate() {
            OnFixedUpdate();
        }
        protected virtual void OnFixedUpdate() { }

        public void Update() {
            UpdatePlaylist();
            OnUpdate();
        }
        protected virtual void OnUpdate() { }

        public void LateUpdate() {
            OnLateUpdate();
        }
        protected virtual void OnLateUpdate() { }

        public bool TrySetMetadata(string key, string value)
        {
            // We can only change metadata as the server!
            if (NetworkInfo.IsServer) {
                GamemodeSender.SendGamemodeMetadataSet(Tag.Value, key, value);
                return true;
            }

            return false;
        }

        public bool TryRemoveMetadata(string key) {
            // We can only remove metadata as the server!
            if (NetworkInfo.IsServer && _internalMetadata.ContainsKey(key)) {
                GamemodeSender.SendGamemodeMetadataRemove(Tag.Value, key);
                return true;
            }

            return false;
        }

        public bool TryInvokeTrigger(string value)
        {
            // We can only invoke triggers as the server!
            if (NetworkInfo.IsServer)
            {
                GamemodeSender.SendGamemodeTriggerResponse(Tag.Value, value);
                return true;
            }

            return false;
        }

        public bool TryGetMetadata(string key, out string value) {
            return _internalMetadata.TryGetValue(key, out value);
        }

        public string GetMetadata(string key) {
            if (_internalMetadata.TryGetValue(key, out string value))
                return value;

            return null;
        }

        internal void Internal_ForceSetMetadata(string key, string value)
        {
            if (_internalMetadata.ContainsKey(key))
                _internalMetadata[key] = value;
            else
                _internalMetadata.Add(key, value);

            OnMetadataChanged(key, value);
            OnInternalMetadataChanged(key, value);
        }

        internal void Internal_ForceRemoveMetadata(string key) {
            if (_internalMetadata.ContainsKey(key)) {
                OnMetadataRemoved(key);

                _internalMetadata.Remove(key);
            }
        }

        internal void Internal_TriggerEvent(string value) {
            OnEventTriggered(value);
        }

        private void OnInternalMetadataChanged(string key, string value) {
            switch (key) {
                case GamemodeHelper.IsStartedKey:
                    Internal_SetGamemodeState(value == bool.TrueString);
                    break;
            }
        }

        protected virtual void OnMetadataChanged(string key, string value) { }

        protected virtual void OnMetadataRemoved(string key) { }

        protected virtual void OnEventTriggered(string value) { }
    }
}
