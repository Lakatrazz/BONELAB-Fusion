using LabFusion.Data;
using LabFusion.Marrow;
using LabFusion.Scene;

using UnityEngine;
using UnityEngine.UI;

using Il2CppTMPro;

using Il2CppSLZ.Marrow.Data;
using Il2CppSLZ.Marrow.Pool;

namespace LabFusion.Utilities;

public static class FusionBitPopup
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
        _bitQueue.Enqueue(amount);
    }

    private static void DequeueBit()
    {
        int amount = _bitQueue.Dequeue();

        var camera = RigData.Refs.ControllerRig.m_head;

        var spawnable = new Spawnable()
        {
            crateRef = FusionSpawnableReferences.BitPopupReference,
            policyData = null,
        };

        AssetSpawner.Register(spawnable);

        SafeAssetSpawner.Spawn(spawnable, Vector3.zero, Quaternion.identity, (poolee) =>
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

            FusionAudio.Play2D(FusionContentLoader.BitGet.Asset, 1f);

            PooleeHelper.DespawnDelayed(poolee, DefaultDuration + 0.1f);
        });
    }
}