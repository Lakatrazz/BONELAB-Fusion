using HarmonyLib;

using LabFusion.Network;
using LabFusion.Bonelab.Extenders;
using LabFusion.MonoBehaviours;
using LabFusion.Data;
using LabFusion.Marrow;
using LabFusion.Utilities;
using LabFusion.Bonelab.Messages;

using Il2CppSLZ.Interaction;
using Il2CppSLZ.Marrow;

namespace LabFusion.Bonelab.Patching;

[HarmonyPatch(typeof(KeyReceiver))]
public static class KeyReceiverPatches
{
    public static bool IgnorePatches { get; set; } = false;

    public static readonly ComponentHashTable<KeyReceiver> HashTable = new();

    [HarmonyPrefix]
    [HarmonyPatch(nameof(KeyReceiver.Awake))]
    public static void AwakePrefix(KeyReceiver __instance)
    {
        var hash = GameObjectHasher.GetHierarchyHash(__instance.gameObject);

        var index = HashTable.AddComponent(hash, __instance);

#if DEBUG
        if (index > 0)
        {
            FusionLogger.Log($"KeyReceiver {__instance.name} had a conflicting hash {hash} and has been added at index {index}.");
        }
#endif

        __instance.gameObject.AddComponent<DestroySensor>().Hook(() =>
        {
            HashTable.RemoveComponent(__instance);
        });
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(KeyReceiver.OnInteractableHostEnter))]
    public static void OnInteractableHostEnter(KeyReceiver __instance, InteractableHost host)
    {
        if (IgnorePatches)
        {
            IgnorePatches = false;
            return;
        }

        if (!NetworkInfo.HasServer)
        {
            return;
        }

        bool inserting = __instance._State == KeyReceiver._States.HOVERING && __instance._keyHost == host;

        if (!inserting)
        {
            return;
        }

        var key = host.gameObject.GetComponentInChildren<Key>(true);

        if (!key)
        {
            return;
        }

        var keyEntity = KeyExtender.Cache.Get(key);

        if (keyEntity == null || !keyEntity.IsRegistered)
        {
            return;
        }

        var data = new KeySlotData()
        {
            KeyId = keyEntity.ID,
            ReceiverData = ComponentPathData.CreateFromComponent<KeyReceiver, KeyReceiverExtender>(__instance, HashTable, KeyReceiverExtender.Cache),
        };

        MessageRelay.RelayModule<KeySlotMessage, KeySlotData>(data, CommonMessageRoutes.ReliableToOtherClients);
    }
}
