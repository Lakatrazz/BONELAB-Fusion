using LabFusion.Extensions;
using LabFusion.SDK.Points;
using LabFusion.Marrow.Integration;

using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Warehouse;

using UnityEngine;

namespace LabFusion.SDK.Cosmetics;

public class CosmeticItem : PointItem
{
    public override string Title => Variables.Title;

    public override string Description => Variables.Description;

    public override int Price => Variables.Price;

    public override string Author => Variables.Author;

    public override string Category => Variables.Category;

    public override string[] Tags => Variables.Tags;

    public override string Barcode => Variables.Barcode;

    public override bool Redacted => Variables.HiddenInShop;

    // We use LateUpdate to cleanup accessories, so it should be hooked
    public override bool ImplementLateUpdate => true;

    protected Dictionary<RigManager, CosmeticInstance> _accessoryInstances = new(new UnityComparer());

    private CosmeticVariables _variables = default;

    private SpawnableCrateReference _spawnableCrateReference = null;

    public CosmeticVariables Variables => _variables;

    public CosmeticItem(CosmeticVariables variables)
    {
        _variables = variables;

        _spawnableCrateReference = new(variables.Barcode);
    }

    public override void LoadPreviewIcon(Action<Texture2D> onLoaded)
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

            var previewIcon = root.previewIcon.Get();

            onLoaded(previewIcon);
        };

        crate.MainGameObject.LoadAsset(onGameObjectLoaded);
    }

    public override void OnUpdateObjects(PointItemPayload payload, bool isVisible)
    {
        // Make sure we have a prefab
        if (isVisible && _spawnableCrateReference.Crate == null)
            return;

        // Check if this is a mirror payload
        if (payload.type == PointItemPayloadType.MIRROR)
        {
            // Make sure we have an accessory instance of this
            if (!_accessoryInstances.ContainsKey(payload.rigManager))
                return;

            if (isVisible)
            {
                Transform tempParent = new GameObject("Temp Parent").transform;
                tempParent.gameObject.SetActive(false);

                var onLoaded = (GameObject go) =>
                {
                    var accessory = GameObject.Instantiate(go, tempParent);
                    accessory.SetActive(false);
                    accessory.transform.parent = null;

                    GameObject.Destroy(tempParent.gameObject);

                    accessory.name = $"{go.name} (Mirror)";

                    _accessoryInstances[payload.rigManager].InsertMirror(payload.mirror, accessory);
                };
                _spawnableCrateReference.Crate.MainGameObject.LoadAsset(onLoaded);
            }
            else
            {
                _accessoryInstances[payload.rigManager].RemoveMirror(payload.mirror);
            }

            return;
        }

        // Make sure we have a rig
        if (payload.rigManager == null)
            return;

        // Check if we need to destroy or create an accessory
        if (isVisible && !_accessoryInstances.ContainsKey(payload.rigManager))
        {
            var onLoaded = (GameObject go) =>
            {
                var accessory = GameObject.Instantiate(go);
                accessory.name = go.name;

                var instance = new CosmeticInstance(payload, accessory, payload.type == PointItemPayloadType.SELF && Variables.HiddenInView, Variables.CosmeticPoint);
                _accessoryInstances.Add(payload.rigManager, instance);
            };
            _spawnableCrateReference.Crate.MainGameObject.LoadAsset(onLoaded);
        }
        else if (!isVisible && _accessoryInstances.ContainsKey(payload.rigManager))
        {
            var instance = _accessoryInstances[payload.rigManager];
            instance.Cleanup();
            _accessoryInstances.Remove(payload.rigManager);
        }
    }

    public override void OnLateUpdate()
    {
        // Make sure theres accessory instances
        if (_accessoryInstances.Count <= 0)
        {
            return;
        }

        // Check if all instances are valid. Otherwise, clean them up
        List<CosmeticInstance> accessoriesToRemove = null;

        foreach (var instance in _accessoryInstances)
        {
            if (!instance.Value.IsValid())
            {
                accessoriesToRemove ??= new List<CosmeticInstance>();

                accessoriesToRemove.Add(instance.Value);
            }
        }

        if (accessoriesToRemove != null)
        {
            for (var i = 0; i < accessoriesToRemove.Count; i++)
            {
                var instance = accessoriesToRemove[i];
                instance.Cleanup();
                _accessoryInstances.Remove(instance.rigManager);
            }
        }
    }
}
