using LabFusion.Marrow;
using LabFusion.SDK.Equippables;
using LabFusion.Extensions;

namespace LabFusion.SDK.Wearables;

public static class WristWatchManager
{
    public static IWearableUIProvider ActiveUI
    {
        get => _activeUI;
        private set
        {
            if (_activeUI == value)
            {
                return;
            }

            _activeUI = value;

            UpdateWatchActivity();

            ActiveUIChanged?.InvokeSafe(value, "executing ActiveUIChanged event");
        }
    }

    public static bool IsWatchActive
    {
        get => _isWatchActive;
        private set
        {
            if (_isWatchActive == value)
            {
                return;
            }

            _isWatchActive = value;

            EquipWristWatch(value);
        }
    }

    public static event Action<IWearableUIProvider> ActiveUIChanged;

    public static event Action WatchBeeped;

    public static string WristWatchBarcode => FusionSpawnableReferences.WristWatchReference.Barcode.ID;

    private static IWearableUIProvider _activeUI = null;
    private static bool _isWatchActive = false;

    public static void EquipWristWatch(bool equip = true) => EquippableManager.EquipEquippable(WristWatchBarcode, equip);

    public static void QueueUI(IWearableUIProvider wearableUIProvider)
    {
        ActiveUI = wearableUIProvider;
    }

    public static void DequeueUI(IWearableUIProvider wearableUIProvider)
    {
        if (wearableUIProvider == ActiveUI)
        {
            ActiveUI = null;
        }
    }

    public static void BeepWatch()
    {
        WatchBeeped?.InvokeSafe("executing WatchBeeped event");
    }

    internal static void Initialize()
    {
        CreateWristWatch();
    }

    private static void UpdateWatchActivity()
    {
        IsWatchActive = ActiveUI != null;
    }

    private static void CreateWristWatch()
    {
        var wristWatch = new WristWatchItem();

        EquippableManager.RegisterEquippable(wristWatch);
    }
}
