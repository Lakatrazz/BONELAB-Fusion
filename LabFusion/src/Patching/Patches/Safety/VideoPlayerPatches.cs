using HarmonyLib;
using LabFusion.Safety;
using LabFusion.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

namespace LabFusion.Patching
{
    [HarmonyPatch(typeof(VideoPlayer))]
    public static class VideoPlayerPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch("Prepare")]
        public static bool PreparePrefix(VideoPlayer __instance)
        {
            //if is evil dont start preparing
            if(CheckEvilness(__instance)) return false;

            return true;
        }
       
        static bool CheckEvilness(VideoPlayer player)
        {
            string url = player.url;

            string domain;
            if (player.source == VideoSource.Url && URLWhitelistManager.isUrl(url) && !URLWhitelistManager.IsLinkWhitelisted(url, out domain))
            {
                FusionLogger.Warn($"Trying to play potentially dangerous URL. Blocking the url {url}, as the domain {domain} is not whitelisted.");

                player.Stop();
                player.url = string.Empty;
                return true;
            }
            return false;
        }
    }
}
