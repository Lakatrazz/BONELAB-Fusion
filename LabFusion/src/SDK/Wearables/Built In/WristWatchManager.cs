using LabFusion.Marrow;
using LabFusion.SDK.Equippables;
using LabFusion.Extensions;

namespace LabFusion.SDK.Wearables;

/// <summary>
/// Interfaces with the multiplayer wrist watch.
/// </summary>
public static class WristWatchManager
{
    /// <summary>
    /// The current UI provider displayed on the watch.
    /// </summary>
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

    /// <summary>
    /// Whether or not the watch is active and equipped.
    /// </summary>
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

    /// <summary>
    /// Invoked when the <see cref="ActiveUI"/> changes.
    /// </summary>
    public static event Action<IWearableUIProvider> ActiveUIChanged;

    /// <summary>
    /// Invoked when a beep is sent to the watch.
    /// </summary>
    public static event Action WatchBeeped;

    /// <summary>
    /// The barcode for the watch item and spawnable.
    /// </summary>
    public static string WristWatchBarcode => FusionSpawnableReferences.WristWatchReference.Barcode.ID;

    private static IWearableUIProvider _activeUI = null;
    private static bool _isWatchActive = false;

    /// <summary>
    /// Equips the wrist watch. This is typically handled by the presence of a UI panel, so only call this if you know what you're doing.
    /// </summary>
    /// <param name="equip"></param>
    public static void EquipWristWatch(bool equip = true) => EquippableManager.EquipEquippable(WristWatchBarcode, equip);

    /// <summary>
    /// Queues a <see cref="IWearableUIProvider"/> to be displayed on the watch.
    /// </summary>
    /// <param name="wearableUIProvider"></param>
    public static void QueueUI(IWearableUIProvider wearableUIProvider)
    {
        ActiveUI = wearableUIProvider;
    }

    /// <summary>
    /// Removes a <see cref="IWearableUIProvider"/> from the watch.
    /// </summary>
    /// <param name="wearableUIProvider"></param>
    public static void DequeueUI(IWearableUIProvider wearableUIProvider)
    {
        if (wearableUIProvider == ActiveUI)
        {
            ActiveUI = null;
        }
    }

    /// <summary>
    /// Sends a notification beep to the watch that will flash until viewed.
    /// </summary>
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
