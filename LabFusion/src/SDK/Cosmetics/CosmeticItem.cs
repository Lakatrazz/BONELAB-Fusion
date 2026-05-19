using Il2CppSLZ.Marrow.Warehouse;

using LabFusion.Marrow.Integration;
using LabFusion.Player;
using LabFusion.SDK.Equippables;
using LabFusion.SDK.Points;
using LabFusion.SDK.Wearables;

using UnityEngine;

namespace LabFusion.SDK.Cosmetics;

// TODO: Cleanup mess after point item system is cleaned up
public class CosmeticItem : WearableItem, IPointItem
{
    public string Title => Variables.Title;

    public string Description => Variables.Description;

    public int Price => Variables.Price;

    public string Author => Variables.Author;

    public string Category => Variables.Category;

    public string[] Tags => Variables.Tags;

    public bool Redacted => Variables.HiddenInShop;

    private CosmeticVariables _variables = default;

    private SpawnableCrateReference _spawnableCrateReference = null;

    public CosmeticVariables Variables => _variables;

    public override string Barcode => Variables.Barcode;

    public CosmeticItem(CosmeticVariables variables)
    {
        _variables = variables;

        _spawnableCrateReference = new(variables.Barcode);
    }

    public void LoadIcon(Action<Texture2D> loadCallback)
    {
        var crate = _spawnableCrateReference.Crate;

        if (crate == null)
        {
            return;
        }

        var onGameObjectLoaded = (GameObject go) =>
        {
            var root = go.GetComponent<CosmeticRoot>();

            if (root == null)
            {
                return;
            }

            var previewIcon = root.PreviewIcon.Get();

            loadCallback(previewIcon);
        };

        crate.MainGameObject.LoadAsset(onGameObjectLoaded);
    }

    public void OnRegistered()
    {
        EquippableManager.RegisterEquippable(this);
    }

    public void OnUnregistered()
    {
    }

    public void OnEquipChanged(PlayerID playerID, bool equipped)
    {
        bool isMe = playerID == null || playerID.IsMe;

        if (!isMe)
        {
            return;
        }

        EquippableManager.EquipEquippable(Barcode, equipped);
    }

    public override WearableInstance CreateInstance()
    {
        return new WearableInstance()
        {
            Anchor = Variables.Anchor,
            HiddenInView = Variables.HiddenInView,
            SpawnableCrateReference = _spawnableCrateReference,
        };
    }
}
