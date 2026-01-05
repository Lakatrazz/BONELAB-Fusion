using Il2CppInterop.Runtime.Attributes;

using LabFusion.Audio;
using LabFusion.Marrow;

using MelonLoader;

using UnityEngine;

namespace LabFusion.Voice;

[RegisterTypeInIl2Cpp]
public class VoiceSource : MonoBehaviour
{
    public VoiceSource(IntPtr intPtr) : base(intPtr) { }

    [HideFromIl2Cpp]
    public static event Action<VoiceSource> OnVoiceEnabled, OnVoiceDisabled;

    [HideFromIl2Cpp]
    public AudioSource AudioSource { get; private set; } = null;

    [HideFromIl2Cpp]
    public AudioStreamFilter StreamFilter { get; private set; } = null;

    private bool _overrideFilter = false;
    public bool OverrideFilter
    {
        get
        {
            return _overrideFilter;
        }
        set
        {
            if (_overrideFilter == value)
            {
                return;
            }

            _overrideFilter = value;

            StreamFilter.enabled = Playing && !value;
        }
    }

    [HideFromIl2Cpp]
    public float Amplitude { get; set; } = 0f;

    private bool _muted = false;
    public bool Muted
    {
        get
        {
            return _muted;
        }
        set
        {
            if (_muted == value)
            {
                return;
            }

            _muted = value;

            ProcessPlaying();
        }
    }

    private bool _receivingInput = false;
    public bool ReceivingInput
    {
        get
        {
            return _receivingInput;
        }
        set
        {
            if (_receivingInput == value)
            {
                return;
            }

            _receivingInput = value;

            _timeSinceInput = 0f;

            ProcessPlaying();
        }
    }

    private float _timeSinceInput = 0f;
    public float TimeSinceInput => _timeSinceInput;

    private bool _wasPlaying = false;
    public bool Playing => ReceivingInput && !Muted;

    public int ID { get; set; } = -1;

    private void Awake()
    {
        GetAudioSource();

        StopSources();
    }

    private void OnEnable()
    {
        OnVoiceEnabled?.Invoke(this);

        ReceivingInput = false;

        StreamFilter.enabled = true;
        StreamFilter.enabled = false;
    }

    private void OnDisable()
    {
        OnVoiceDisabled?.Invoke(this);

        ReceivingInput = false;

        StreamFilter.enabled = false;
    }

    private void Update()
    {
        if (!Playing)
        {
            return;
        }

        float deltaTime = Time.deltaTime;

        StreamFilter.TickPeak(deltaTime);

        if (StreamFilter.ReadingQueue.Count <= 0)
        {
            _timeSinceInput += deltaTime;
        }

        if (_timeSinceInput > 1f)
        {
            ReceivingInput = false;
        }
    }

    [HideFromIl2Cpp]
    private void ProcessPlaying()
    {
        if (_wasPlaying == Playing)
        {
            return;
        }

        _wasPlaying = Playing;

        if (Playing)
        {
            PlaySources();
        }
        else
        {
            StopSources();
        }
    }

    private void PlaySources()
    {
        StreamFilter.enabled = !OverrideFilter;
        AudioSource.Play();

        Amplitude = 0f;
    }

    private void StopSources()
    {
        AudioSource.Stop();
        StreamFilter.enabled = false;

        StreamFilter.ClearValues();

        Amplitude = 0f;
    }

    [HideFromIl2Cpp]
    private void GetAudioSource()
    {
        var existingSource = GetComponent<AudioSource>();

        if (existingSource != null)
        {
            AudioSource = existingSource;
        }
        else
        {
            AudioSource = CreateAudioSource();
        }

        SetSourceSettings();
    }

    [HideFromIl2Cpp]
    private AudioSource CreateAudioSource()
    {
        var source = gameObject.AddComponent<AudioSource>();

        source.volume = 1f;
        source.spatialBlend = 1f;
        source.dopplerLevel = 0.5f;
        source.spread = 60f;

        return source;
    }

    [HideFromIl2Cpp]
    private void SetSourceSettings()
    {
        AudioSource.loop = true;
        AudioSource.playOnAwake = false;
        AudioSource.outputAudioMixerGroup = LocalAudioPlayer.Vocals;

        // Regular reverb zones apply before audio filters, so they don't work for the voice filter
        AudioSource.bypassReverbZones = true;
        AudioSource.reverbZoneMix = 0f;

        StreamFilter = gameObject.AddComponent<AudioStreamFilter>();

        AudioSource.clip = AudioInfo.CreateToneClip();
    }
}
