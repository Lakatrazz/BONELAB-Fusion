using Il2CppSLZ.Marrow.Audio;

using LabFusion.Extensions;
using LabFusion.Marrow;

namespace LabFusion.SDK.Gamemodes;

public class MusicPlaylist
{
    private AudioReference[] _songs = null;
    public AudioReference[] Songs => _songs;

    private int _currentTrack = -1;
    public int CurrentTrack => _currentTrack;

    private float _volume = SafeAudio3dPlayer.MusicVolume;
    public float Volume
    {
        get { return _volume; }
        set { _volume = value; }
    }

    private bool _loopSingle = false;
    public bool LoopSingle { get { return _loopSingle; } set { _loopSingle = value; } }

    private bool _isActive = false;
    public bool IsActive => _isActive;

    public void StartPlaylist()
    {
        _isActive = true;

        NextTrack();
    }

    public void StopPlaylist()
    {
        StopMusic();

        _isActive = false;
    }

    private void PlayMusic()
    {
        var currentTrack = GetCurrentTrack();

        currentTrack.LoadClip((clip) =>
        {
            Audio2dPlugin.Audio2dManager.CueOverrideMusic(clip, Volume, 0.2f, 0.2f, LoopSingle, false);
        });
    }

    private void StopMusic()
    {
        var audio2dManager = Audio2dPlugin.Audio2dManager;

        bool noOriginalMusic = audio2dManager._overridenMusicClip == null;

        audio2dManager.StopOverrideMusic();

        // Incase no music was overriden, we need to manually stop the music
        // This way the music doesn't just keep playing anyways
        if (noOriginalMusic)
        {
            audio2dManager.StopMusic(0.2f);
        }
    }

    public void NextTrack()
    {
        if (!IsActive)
        {
            return;
        }

        if (Songs == null)
        {
            _currentTrack = -1;
            return;
        }

        _currentTrack++;

        if (_currentTrack >= Songs.Length)
        {
            _currentTrack = 0;
        }

        PlayMusic();
    }

    public void PreviousTrack()
    {
        if (!IsActive)
        {
            return;
        }

        if (Songs == null)
        {
            _currentTrack = -1;
            return;
        }

        _currentTrack--;

        if (_currentTrack < 0)
        {
            _currentTrack = Songs.Length - 1;
        }

        PlayMusic();
    }

    public AudioReference GetCurrentTrack()
    {
        if (CurrentTrack < 0)
        {
            return default;
        }

        if (Songs == null)
        {
            return default;
        }

        return Songs[CurrentTrack];
    }

    public void Update()
    {
        // Make sure we're currently playing tracks
        if (!IsActive)
        {
            return;
        }

        // Check if we're ready to move tracks
        bool isOverridingTrack = Audio2dPlugin.Audio2dManager._isOverride;
        bool isPlayingTrack = isOverridingTrack;

        // Check if the track has ended
        if (isOverridingTrack)
        {
            var ambAndMusic = GetCurrentAmbAndMusic();

            if (ambAndMusic == null || !ambAndMusic.ambMus.isPlaying)
            {
                isPlayingTrack = false;
            }
        }

        if (!isPlayingTrack)
        {
            NextTrack();
        }
    }

    private static AmbAndMusic GetCurrentAmbAndMusic()
    {
        var audioManager = Audio2dPlugin.Audio2dManager;
        var curMus = audioManager._curMus;

        if (curMus > 0 && curMus < audioManager.ambAndMusics.Length)
        {
            return audioManager.ambAndMusics[curMus];
        }

        return null;
    }

    public void SetPlaylist(params AudioReference[] songs)
    {
        _songs = songs;
        _currentTrack = -1;
    }

    public void Shuffle()
    {
        if (Songs == null)
        {
            return;
        }

        Songs.Shuffle();
    }
}