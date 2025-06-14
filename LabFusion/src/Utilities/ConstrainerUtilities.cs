using UnityEngine;

using Il2CppSLZ.Marrow.Warehouse;
using Il2CppSLZ.Marrow;

using LabFusion.Extensions;
using LabFusion.Network;
using LabFusion.Marrow;
using LabFusion.Bonelab;

namespace LabFusion.Utilities;

public static class ConstrainerUtilities
{
    public static bool PlayerConstraintsEnabled { get { return LobbyInfoManager.LobbyInfo.PlayerConstraining; } }

    public static bool HasConstrainer { get { return GlobalConstrainer != null; } }
    public static Constrainer GlobalConstrainer { get; private set; }

    private static Action _constrainerCreatedCallback = null;

    public const string ConstrainerAssetGUID = "bf9c97bf88c22dc4f981578e75d9aa12";

    public static void OnMainSceneInitialized()
    {
        // Get the constrainer crate so we can create a global constrainer
        var crate = CrateFilterer.GetCrate<SpawnableCrate>(BonelabSpawnableReferences.ConstrainerReference.Barcode);

        if (crate == null)
        {
            return;
        }

        // If this was replaced, fix the GameObject GUID
        if (!crate.Pallet.IsInMarrowGame())
        {
            crate.MainAsset = new MarrowAsset(ConstrainerAssetGUID);
        }

        // Load the asset so we can create it
        var loadCallback = OnConstrainerLoaded;

        crate.LoadAsset(loadCallback);
    }

    private static void OnConstrainerLoaded(GameObject go)
    {
        if (go == null)
        {
            _constrainerCreatedCallback = null;
            return;
        }

        var constrainer = GameObject.Instantiate(go, new Vector3(1000f, 1000f, 1000f), QuaternionExtensions.identity);
        GlobalConstrainer = constrainer.GetComponent<Constrainer>();
        constrainer.SetActive(false);

        _constrainerCreatedCallback?.InvokeSafe("executing Constrainer Created callback");
        _constrainerCreatedCallback = null;
    }

    public static void HookConstrainerCreated(Action callback)
    {
        if (HasConstrainer)
        {
            callback?.Invoke();
        }
        else
        {
            _constrainerCreatedCallback += callback;
        }
    }
}