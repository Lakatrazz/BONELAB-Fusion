﻿using LabFusion.Data;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

namespace LabFusion.Utilities
{
    public static class FusionBitPopup
    {
        private static readonly Queue<int> _bitQueue = new();

        public const float DefaultDuration = 1f;
        public static readonly Vector3 LocalPosition = new(0.319f, -0.198f, 0.783f);
        public static readonly Quaternion LocalRotation = Quaternion.Euler(0f, 17.252f, 0f);

        internal static void OnUpdate() {
            if (FusionSceneManager.HasTargetLoaded() && !FusionSceneManager.IsDelayedLoading() && RigData.HasPlayer && _bitQueue.Count > 0) {
                DequeueBit();
            }
        }

        public static void Send(int amount) {
            _bitQueue.Enqueue(amount);
        }

        private static void DequeueBit() {
            int amount = _bitQueue.Dequeue();

            var camera = RigData.RigReferences.ControllerRig.m_head;
            GameObject instance = GameObject.Instantiate(FusionContentLoader.BitPopupPrefab, camera);
            UIMachineUtilities.OverrideFonts(instance.transform);

            instance.transform.localPosition = LocalPosition;
            instance.transform.localRotation = LocalRotation;

            Transform canvas = instance.transform.Find("Offset/Canvas");

            string text = amount < 0 ? $"{amount}" : $"+{amount}";
            canvas.Find("text_shadow").GetComponent<TMP_Text>().text = text;

            var amountText = canvas.Find("amount").GetComponent<TMP_Text>();
            amountText.text = text;

            if (amount < 0)
            {
                amountText.color = Color.red;
                canvas.Find("bit").GetComponent<RawImage>().color = Color.red;
            }

            FusionAudio.Play2D(FusionContentLoader.BitGet, 1f);

            GameObject.Destroy(instance, DefaultDuration + 0.1f);
        }
    }
}
