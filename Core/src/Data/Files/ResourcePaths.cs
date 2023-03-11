using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Data
{
    public static class ResourcePaths {
        public const string SteamAPIPath = "LabFusion.Core.resources.lib.x86_64.steam_api64.dll";

        public const string AppDataSubFolder = "/Fusion/";

        public const string WindowsBundlePrefix = "LabFusion.Core.resources.bundles.StandaloneWindows64.";
        public const string AndroidBundlePrefix = "LabFusion.Core.resources.bundles.Android.";

        // Item bundle
        public const string ItemBundle = "item_bundle.fusion";

        public const string ItemPrefix = "item_";
        public const string PreviewPrefix = "preview_";

        // Content bundle
        public const string ContentBundle = "content_bundle.fusion";

        public const string PointShopPrefab = "machine_PointShop";
        public const string InfoBoxPrefab = "machine_InfoBox";

        public const string EntangledLinePrefab = "renderer_EntangledLine";

        public const string SabrelakeLogo = "tex_sabreLakeLogo";
        public const string LavaGangLogo = "tex_lavaGangLogo";

        public const string LavaGangVictory = "sting_LavaGangVictory";
        public const string SabrelakeVictory = "sting_sabrelakeVictory";

        public const string LavaGangFailure = "sting_LavaGangFailure";
        public const string SabrelakeFailure = "sting_sabrelakeFailure";

        public const string DMTie = "sting_DMTie";

        public const string GeoGrpFellDownTheStairs = "music_GeoGrpFellDownTheStairs";

        public const string UISelect = "UI_Beep_Bend_Short_stereo";
        public const string UIDeny = "UI_Error_Double_Note_Down_Notch_stereo";
        public const string UIConfirm = "UI_SCI-FI_Confirm_Dry_stereo";

        public const string PurchaseFailure = "stinger_FailPurchase";
        public const string PurchaseSuccess = "stinger_SuccessPurchase";

        public const string EquipItem = "ui_EquipItem";
        public const string UnequipItem = "ui_UnequipItem";
    }
}
