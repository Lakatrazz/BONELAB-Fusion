using LabFusion.Extensions;
using LabFusion.SDK.Points;
using LabFusion.Utilities;
using SLZ.Rig;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace LabFusion.SDK.Points
{
    public abstract class AccessoryItem : PointItem {
        protected class AccessoryInstance {
            public RigManager rigManager;

            public GameObject accessory;
            public Transform transform;

            public bool isHiddenInView;

            public Dictionary<Mirror, GameObject> mirrors = new Dictionary<Mirror, GameObject>(new UnityComparer());

            public AccessoryInstance(PointItemPayload payload, GameObject accessory, bool isHiddenInView) {
                rigManager = payload.rigManager;

                this.accessory = accessory;
                transform = accessory.transform;

                this.isHiddenInView = isHiddenInView;
            }
            
            public void Update(AccessoryPoint itemPoint, AccessoryScaleMode mode) {
                if (Time.timeScale > 0f && isHiddenInView)
                    accessory.SetActive(false);
                else
                    accessory.SetActive(true);

                AccessoryItemHelper.GetTransform(itemPoint, mode, rigManager, out var position, out var rotation, out var scale);
                
                transform.SetPositionAndRotation(position, rotation);
                transform.localScale = scale;
            }

            public void InsertMirror(Mirror mirror, GameObject accessory) {
                if (mirrors.ContainsKey(mirror))
                    return;

                mirrors.Add(mirror, accessory);
            }

            public void RemoveMirror(Mirror mirror) {
                if (mirrors.TryGetValue(mirror, out var accessory)) {
                    mirrors.Remove(mirror);

                    GameObject.Destroy(accessory);
                }
            }

            public void UpdateMirrors() {
                List<Mirror> mirrorsToRemove = null;

                // Update all mirrors
                foreach (var mirror in mirrors) {
                    if (mirror.Key && mirror.Value) {
                        // Remove the mirror if its disabled
                        if (!mirror.Key.isActiveAndEnabled) {
                            if (mirrorsToRemove == null)
                                mirrorsToRemove = new List<Mirror>();

                            mirrorsToRemove.Add(mirror.Key);
                            continue;
                        }

                        // Set the mirror accessory active or inactive
                        if (mirror.Key.rigManager != rigManager){
                            mirror.Value.SetActive(false);
                        }
                        else {
                            mirror.Value.SetActive(true);

                            Transform reflectTran = mirror.Key._reflectTran;

                            Transform accessory = mirror.Value.transform;

                            // Get rotation
                            Vector3 forward = transform.forward;
                            Vector3 up = transform.up;

                            Vector3 reflectLine = reflectTran.forward;

                            forward = Vector3.Reflect(forward, reflectLine);
                            up = Vector3.Reflect(up, reflectLine);

                            Quaternion rotation = Quaternion.LookRotation(forward, up);

                            // Get position
                            Vector3 position = transform.position - reflectTran.position;
                            position = Vector3.Reflect(position, reflectTran.forward);
                            position += reflectTran.position;

                            // Set position, rotation, and scale
                            accessory.SetPositionAndRotation(position, rotation);
                            accessory.localScale = transform.localScale;
                        }
                    }
                }

                // Remove necessary mirrors
                if (mirrorsToRemove != null) {
                    foreach (var mirror in mirrorsToRemove)
                        RemoveMirror(mirror);
                }
            }

            public bool IsValid() {
                return !rigManager.IsNOC() && !accessory.IsNOC();
            }

            public void Cleanup() {
                if (!accessory.IsNOC())
                    GameObject.Destroy(accessory);

                foreach (var mirror in mirrors) {
                    if (!mirror.Value.IsNOC())
                        GameObject.Destroy(mirror.Value);
                }

                mirrors.Clear();
            }
        }

        // The location of the item
        public virtual AccessoryPoint ItemPoint => AccessoryPoint.HEAD_TOP;

        // Required getter for the instantiated accessory prefab.
        public abstract GameObject AccessoryPrefab { get; }

        // How should the accessory scale with the player?
        public virtual AccessoryScaleMode ScaleMode => AccessoryScaleMode.HEIGHT;

        // Is the accessory hidden from the local view?
        public virtual bool IsHiddenInView => false;

        // We use LateUpdate to update object positions, so it should be hooked
        public override bool ImplementLateUpdate => true;

        protected Dictionary<RigManager, AccessoryInstance> _accessoryInstances = new Dictionary<RigManager, AccessoryInstance>(new UnityComparer());

        public override void OnUpdateObjects(PointItemPayload payload, bool isVisible) {
            // Make sure we have a prefab
            if (isVisible && AccessoryPrefab == null)
                return;

            // Check if this is a mirror payload
            if (payload.type == PointItemPayloadType.MIRROR) {
                // Make sure we have an accessory instance of this
                if (!_accessoryInstances.ContainsKey(payload.rigManager))
                    return;

                if (isVisible) {
                    Transform tempParent = new GameObject("Temp Parent").transform;
                    tempParent.gameObject.SetActive(false);

                    var accessory = GameObject.Instantiate(AccessoryPrefab, tempParent);
                    accessory.SetActive(false);
                    accessory.transform.parent = null;

                    GameObject.Destroy(tempParent.gameObject);

                    accessory.name = $"{AccessoryPrefab.name} (Mirror)";

                    _accessoryInstances[payload.rigManager].InsertMirror(payload.mirror, accessory);
                }
                else {
                    _accessoryInstances[payload.rigManager].RemoveMirror(payload.mirror);
                }

                return;
            }

            // Make sure we have a rig
            if (payload.rigManager == null)
                return;

            // Check if we need to destroy or create an accessory
            if (isVisible && !_accessoryInstances.ContainsKey(payload.rigManager)) {
                var accessory = GameObject.Instantiate(AccessoryPrefab);
                accessory.name = AccessoryPrefab.name;

                var instance = new AccessoryInstance(payload, accessory, payload.type == PointItemPayloadType.SELF && IsHiddenInView);
                _accessoryInstances.Add(payload.rigManager, instance);
            }
            else if (!isVisible && _accessoryInstances.ContainsKey(payload.rigManager)) {
                var instance = _accessoryInstances[payload.rigManager];
                instance.Cleanup();
                _accessoryInstances.Remove(payload.rigManager);
            }
        }

        public override void OnLateUpdate() {
            if (_accessoryInstances.Count <= 0)
                return;

            List<AccessoryInstance> accessoriesToRemove = null;

            foreach (var instance in _accessoryInstances) {
                if (!instance.Value.IsValid()) {
                    if (accessoriesToRemove == null)
                        accessoriesToRemove = new List<AccessoryInstance>();

                    accessoriesToRemove.Add(instance.Value);
                    continue;
                }

                instance.Value.Update(ItemPoint, ScaleMode);

                instance.Value.UpdateMirrors();
            }

            if (accessoriesToRemove != null) {
                for (var i = 0; i < accessoriesToRemove.Count; i++) {
                    var instance = accessoriesToRemove[i];
                    instance.Cleanup();
                    _accessoryInstances.Remove(instance.rigManager);
                }
            }
        }
    }
}
