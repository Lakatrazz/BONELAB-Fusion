using HarmonyLib;

using LabFusion.Network;
using LabFusion.Senders;
using LabFusion.Utilities;

using Il2CppSLZ.Marrow.Warehouse;
using Il2CppSLZ.Bonelab;

namespace LabFusion.Patching
{
    [HarmonyPatch(typeof(LevelsPanelView), nameof(LevelsPanelView.SelectItem))]
    public class LevelsPanelViewPatches
    {
        public static bool Prefix(LevelsPanelView __instance, int idx)
        {
            try
            {
                // Prevent the menu from loading a different level if we aren't the host
                if (NetworkInfo.HasServer && !NetworkInfo.IsServer)
                {
                    // Send level request
                    LevelCrate crate = __instance._levelCrates[idx + (__instance._currentPage * __instance.items.Count)];
                    LoadSender.SendLevelRequest(crate);

                    // Notify the user they've requested a level
                    FusionNotifier.Send(new FusionNotification()
                    {
                        title = "Requested Level",
                        message = $"Sent a level request for {crate.Title}!",
                        isMenuItem = false,
                        isPopup = true,
                    });

                    return false;
                }
            }
            catch (Exception e)
            {
#if DEBUG
                FusionLogger.LogException("to execute patch LevelsPanelView.SelectItem", e);
#endif
            }

            return true;
        }
    }
}
