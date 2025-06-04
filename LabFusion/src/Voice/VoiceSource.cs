using Il2CppInterop.Runtime.Attributes;

using LabFusion.Audio;

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
    }

    private void OnEnable()
    {
        OnVoiceEnabled?.Invoke(this);

        ReceivingInput = false;
    }

    private void OnDisable()
    {
        OnVoiceDisabled?.Invoke(this);

        ReceivingInput = false;
    }

    private void Update()
    {
        if (!Playing)
        {
            return;
        }

        if (StreamFilter.ReadingQueue.Count <= 0 || Amplitude < VoiceVolume.MinimumVoiceVolume)
        {
            _timeSinceInput += Time.deltaTime;
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
            StreamFilter.enabled = true;
            AudioSource.Play();

            Amplitude = 0f;
        }
        else
        {
            AudioSource.Stop();
            StreamFilter.enabled = false;

            StreamFilter.ReadingQueue.Clear();

            Amplitude = 0f;
        }
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
        source.outputAudioMixerGroup = null;
        source.spatialBlend = 1f;
        source.dopplerLevel = 0.5f;
        source.spread = 60f;

        return source;
    }

    [HideFromIl2Cpp]
    private void SetSourceSettings()
    {
        AudioSource.loop = true;

        StreamFilter = gameObject.AddComponent<AudioStreamFilter>();

        AudioSource.clip = AudioInfo.CreateToneClip();
    }
}
