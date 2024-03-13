using BoneLib.BoneMenu.Elements;
using LabFusion.Preferences;
using LabFusion.Representation;
using LabFusion.Senders;
using LabFusion.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnhollowerBaseLib;
using UnityEngine;

namespace LabFusion.Network
{
    /// <summary>
    /// Internal class used for creating network layers and updating them.
    /// </summary>
    internal static class InternalLayerHelpers
    {
        internal static NetworkLayer CurrentNetworkLayer { get; private set; }

        internal static void SetLayer(NetworkLayer layer)
        {
            CurrentNetworkLayer = layer;
            CurrentNetworkLayer.OnInitializeLayer();
        }

        internal static void OnLateInitializeLayer()
        {
            CurrentNetworkLayer?.OnLateInitializeLayer();
        }

        internal static void OnCleanupLayer()
        {
            CurrentNetworkLayer?.OnCleanupLayer();

            CurrentNetworkLayer = null;
        }

        internal static void OnUpdateLayer()
        {
            CurrentNetworkLayer?.OnUpdateLayer();
        }

        internal static void OnLateUpdateLayer()
        {
            CurrentNetworkLayer?.OnLateUpdateLayer();
        }

        internal static void OnGUILayer()
        {
            CurrentNetworkLayer?.OnGUILayer();
        }

        internal static void OnUpdateLobby()
        {
            CurrentNetworkLayer?.OnUpdateLobby();
        }


        private static int lastSample = 0;
        internal static void OnVoiceChatUpdate()
        {
            if (NetworkInfo.HasServer)
            {
                bool voiceEnabled = VoiceHelper.IsVoiceEnabled;

                if (voiceEnabled && !Microphone.IsRecording(FusionPreferences.ClientSettings.MicrophoneName.GetValue()))
                    NetworkInfo.VoiceManager.VoiceClip = Microphone.Start(FusionPreferences.ClientSettings.MicrophoneName.GetValue(), true, 1, 41000);
                else if (!voiceEnabled && Microphone.IsRecording(FusionPreferences.ClientSettings.MicrophoneName.GetValue()))
                    Microphone.End(null);

                if (voiceEnabled)
                {
                    // Get the Audio Position
                    int position = Microphone.GetPosition(FusionPreferences.ClientSettings.MicrophoneName.GetValue());

                    if (position < lastSample)
                    {
                        lastSample = 0;
                    }

                    int sampleCount = position - lastSample;
                    if (sampleCount < 0)
                    {
                        // Account for audio looping
                        sampleCount += NetworkInfo.VoiceManager.VoiceClip.samples;
                    }

                    Il2CppStructArray<float> audioData = new Il2CppStructArray<float>(sampleCount);
                    NetworkInfo.VoiceManager.VoiceClip.GetData(audioData, lastSample);

                    lastSample = position;

                    byte[] byteArray = new byte[audioData.Length * 4];

                    bool isTalking = false;
                    for (int i = 0; i < audioData.Length; i++)
                    {
                        byte[] converted = BitConverter.GetBytes(audioData[i]);
                        Array.Copy(converted, 0, byteArray, i * 4, 4);

                        // Check for talking
                        if (Math.Abs(audioData[i]) > 0.0001f)
                        {
                            isTalking = true;
                        }
                    }

                    if (isTalking)
                    {
                        CurrentNetworkLayer?.OnVoiceChatUpdate(byteArray);
                    }
                }

                // Update the manager
                CurrentNetworkLayer.VoiceManager.Update();
            }
        }

        internal static void OnVoiceBytesReceived(PlayerId id, byte[] bytes)
        {
            CurrentNetworkLayer?.OnVoiceBytesReceived(id, bytes);
        }

        internal static void OnSetupBoneMenuLayer(MenuCategory category)
        {
            CurrentNetworkLayer?.OnSetupBoneMenu(category);
        }

        internal static void OnUserJoin(PlayerId id)
        {
            CurrentNetworkLayer?.OnUserJoin(id);
        }
    }
}
