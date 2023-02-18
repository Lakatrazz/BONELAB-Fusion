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

namespace LabFusion.SDK.Points
{
    public enum AccessoryPoint {
        HEAD = 0,
        HEAD_TOP = 1,
        EYE_LEFT = 2,
        EYE_RIGHT = 3,
        CHEST = 4,
        HIPS = 5,
        LOCOSPHERE = 6,
    }

    public abstract class AccessoryItem : PointItem {
        protected class AccessoryInstance {
            public RigManager rigManager;
            public ArtRig artRig;
            public PhysicsRig physicsRig;

            public GameObject accessory;
            public Transform transform;
            
            public AccessoryInstance(PointItemPayload payload, GameObject accessory) {
                rigManager = payload.rigManager;

                if (rigManager != null) {
                    artRig = rigManager.artOutputRig;
                    physicsRig = rigManager.physicsRig;
                }

                this.accessory = accessory;
                transform = accessory.transform;
            }
            
            public void Update(AccessoryPoint itemPoint, bool scale) {
                Vector3 position;
                Quaternion rotation;
                Transform head;

                var avatar = rigManager._avatar;

                if (scale) {
                    transform.localScale = Vector3.one * (avatar.height / 1.76f);
                }

                switch (itemPoint) {
                    default:
                    case AccessoryPoint.HEAD:
                        head = artRig.m_head;
                        position = head.position;
                        rotation = head.rotation;
                        break;
                    case AccessoryPoint.HEAD_TOP:
                        head = artRig.m_head;
                        position = head.position + head.up * avatar.HeadTop;
                        rotation = head.rotation;
                        break;
                    case AccessoryPoint.EYE_LEFT:
                        position = artRig.eyeLf.position;
                        rotation = artRig.eyeLf.rotation;
                        break;
                    case AccessoryPoint.EYE_RIGHT:
                        position = artRig.eyeRt.position;
                        rotation = artRig.eyeRt.rotation;
                        break;
                    case AccessoryPoint.CHEST:
                        position = artRig.m_chest.position;
                        rotation = artRig.m_chest.rotation;
                        break;
                    case AccessoryPoint.HIPS:
                        position = artRig.m_pelvis.position;
                        rotation = artRig.m_pelvis.rotation;
                        break;
                    case AccessoryPoint.LOCOSPHERE:
                        position = physicsRig.physG.transform.position;
                        rotation = physicsRig.physG.transform.rotation;
                        break;
                }

                transform.SetPositionAndRotation(position, rotation);
            }

            public bool IsValid() {
                return !rigManager.IsNOC() && !accessory.IsNOC();
            }

            public void Cleanup() {
                if (!accessory.IsNOC())
                    GameObject.Destroy(accessory);
            }
        }

        // The location of the item
        public virtual AccessoryPoint ItemPoint => AccessoryPoint.HEAD_TOP;

        // Required getter for the instantiated accessory prefab.
        public abstract GameObject AccessoryPrefab { get; }

        // Should the accessory scale with the player height? Ford's height is 1, 1, 1 scale
        public virtual bool ScaleWithHeight => true;

        protected Dictionary<RigManager, AccessoryInstance> _accessoryInstances = new Dictionary<RigManager, AccessoryInstance>(new UnityComparer());

        public override void OnUpdateObjects(PointItemPayload payload, bool isVisible) {
            // Make sure we have a prefab
            if (isVisible && AccessoryPrefab == null)
                return;

            // Make sure we have a rig
            if (payload.rigManager == null)
                return;

            // Check if we need to destroy or create an accessory
            if (isVisible && !_accessoryInstances.ContainsKey(payload.rigManager)) {
                var accessory = GameObject.Instantiate(AccessoryPrefab);
                accessory.name = AccessoryPrefab.name;

                var instance = new AccessoryInstance(payload, accessory);
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

            foreach (var instance in _accessoryInstances.ToArray()) {
                if (!instance.Value.IsValid()) {
                    instance.Value.Cleanup();
                    _accessoryInstances.Remove(instance.Key);
                    continue;
                }

                instance.Value.Update(ItemPoint, ScaleWithHeight);
            }
        }
    }
}
