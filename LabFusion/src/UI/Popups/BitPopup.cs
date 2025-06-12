using LabFusion.Data;
using LabFusion.Marrow;
using LabFusion.Scene;
using LabFusion.Utilities;
using LabFusion.Marrow.Pool;

using UnityEngine;
using UnityEngine.UI;

using Il2CppTMPro;

using Il2CppSLZ.Marrow.Data;
using Il2CppSLZ.Marrow.Pool;

namespace LabFusion.UI.Popups;

public static class BitPopup
{
    private static readonly Queue<int> _bitQueue = new();

    public const float DefaultDuration = 1f;
    public static readonly Vector3 LocalPosition = new(0.319f, -0.198f, 0.783f);
    public static readonly Quaternion LocalRotation = Quaternion.Euler(0f, 17.252f, 0f);

    internal static void OnUpdate()
    {
        if (FusionSceneManager.HasTargetLoaded() && !FusionSceneManager.IsDelayedLoading() && RigData.HasPlayer && _bitQueue.Count > 0)
        {
            DequeueBit();
        }
    }

    public static void Send(int amount)
    {
        if (amount == 0)
        {
            return;
        }

        _bitQueue.Enqueue(amount);
    }

    private static void DequeueBit()
    {
        int amount = _bitQueue.Dequeue();

        var camera = RigData.Refs.ControllerRig.m_head;

        var spawnable = LocalAssetSpawner.CreateSpawnable(FusionSpawnableReferences.BitPopupReference);

        LocalAssetSpawner.Register(spawnable);

        LocalAssetSpawner.Spawn(spawnable, Vector3.zero, Quaternion.identity, (poolee) =>
        {
            var popupTransform = poolee.transform;
            popupTransform.parent = camera;

            UIMachineUtilities.OverrideFonts(popupTransform);

            popupTransform.SetLocalPositionAndRotation(LocalPosition, LocalRotation);

            Transform canvas = popupTransform.Find("Offset/Canvas");

            string text = amount < 0 ? $"{amount}" : $"+{amount}";
            canvas.Find("text_shadow").GetComponent<TMP_Text>().text = text;

            var amountText = canvas.Find("amount").GetComponent<TMP_Text>();
            amountText.text = text;

            Color color = Color.white;

            if (amount < 0)
            {
                color = Color.red;
            }

            amountText.color = color;
            canvas.Find("bit").GetComponent<RawImage>().color = color;

            LocalAudioPlayer.Play2dOneShot(new AudioReference(FusionMonoDiscReferences.BitGetReference), LocalAudioPlayer.InHeadSettings);

            PooleeHelper.DespawnDelayed(poolee, DefaultDuration + 0.1f);
        });
    }
}