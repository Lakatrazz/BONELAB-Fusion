using LabFusion.Audio;
using LabFusion.Data;
using LabFusion.Math;
using LabFusion.Utilities;
using LabFusion.Voice;

using UnityEngine;

namespace LabFusion.Entities;

public class RigVoiceSource
{
    public GameObject GameObject { get; private set; } = null;

    public Transform Transform { get; private set; } = null;

    public VoiceSource VoiceSource { get; private set; } = null;

    public AudioLowPassFilter LowPassFilter { get; private set; } = null;

    public const float HighFrequency = 25000f;
    public float Frequency { get; set; } = HighFrequency;
    public float TargetFrequency { get; set; } = HighFrequency;
    public float OcclusionMultiplier { get; set; } = 1f;

    public float MinMicrophoneDistance { get; set; } = 1f;
    public float MaxMicrophoneDistance { get; set; } = 30f;

    public Transform Mouth { get; private set; } = null;

    public JawFlapper JawFlapper { get; set; } = null;

    public RigVoiceSource(JawFlapper jawFlapper, Transform mouth)
    {
        JawFlapper = jawFlapper;
        Mouth = mouth;
    }

    public void CreateVoiceSource(int id)
    {
        VoiceSource = VoiceSourceManager.CreateVoiceSource(id);

        GameObject = VoiceSource.gameObject;
        Transform = GameObject.transform;

        Transform.parent = Mouth;
        Transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

        LowPassFilter = GameObject.AddComponent<AudioLowPassFilter>();
        LowPassFilter.lowpassResonanceQ = 0.1f;
        LowPassFilter.cutoffFrequency = HighFrequency;

        VoiceSource.AudioSource.minDistance = MinMicrophoneDistance;
    }

    public void DestroyVoiceSource()
    {
        if (GameObject == null)
        {
            return;
        }

        GameObject.Destroy(GameObject);
    }

    public void UpdateVoiceSource(float distanceSqr, float deltaTime)
    {
        bool muted = distanceSqr > MaxMicrophoneDistance * MaxMicrophoneDistance * 1.2f;

        VoiceSource.Muted = muted;

        if (muted)
        {
            JawFlapper.ClearJaw();
        }
        else
        {
            JawFlapper.UpdateJaw(VoiceSource.Amplitude, deltaTime);

            UpdateOcclusion(deltaTime);
        }
    }

    private float _occlusionUpdateTimer = 0f;

    public void UpdateOcclusion(float deltaTime)
    {
        _occlusionUpdateTimer += deltaTime;

        if (_occlusionUpdateTimer >= 0.1f)
        {
            _occlusionUpdateTimer = 0f;
            CheckLowPass();
        }

        TargetFrequency = HighFrequency * OcclusionMultiplier;

        Frequency = ManagedMathf.Lerp(Frequency, TargetFrequency, Smoothing.CalculateDecay(24f, TimeUtilities.DeltaTime));

        LowPassFilter.cutoffFrequency = Frequency;
    }

    public void SetVoiceRange(float avatarHeight)
    {
        float heightMult = avatarHeight / 1.76f;

        MinMicrophoneDistance = 3f * MathF.Sqrt(heightMult);
        MaxMicrophoneDistance = 30f * MathF.Sqrt(heightMult);

        if (VoiceSource == null)
        {
            return;
        }

        VoiceSource.AudioSource.minDistance = MinMicrophoneDistance;
    }

    private void CheckLowPass()
    {
        OcclusionMultiplier = 1f;

        if (!RigData.HasPlayer)
        {
            return;
        }

        var listener = RigData.Refs.Headset.position;
        var source = Transform.position;

        OcclusionMultiplier = AudioOcclusion.RaycastOcclusionMultiplier(listener, source);
    }
}
