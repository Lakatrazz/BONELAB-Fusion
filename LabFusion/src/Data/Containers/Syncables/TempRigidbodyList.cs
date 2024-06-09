using LabFusion.Extensions;

using Il2CppSLZ.Interaction;

using UnityEngine;

namespace LabFusion.Data
{
    public sealed class TempRigidbodyInfo
    {
        public GameObject GameObject { get; private set; }
        public Transform Transform { get; private set; }
        public Rigidbody Rigidbody { get; set; }

        public TempRigidbodyInfo(GameObject go, Transform transform, Rigidbody rb)
        {
            this.GameObject = go;
            this.Transform = transform;
            this.Rigidbody = rb;
        }
    }

    public sealed class TempRigidbodyList
    {
        public int Length { get; private set; }
        public TempRigidbodyInfo[] Items { get; private set; }

        public void WriteComponents(GameObject go)
        {
            var rbs = go.GetComponentsInChildren<Rigidbody>(true);
            var hosts = go.GetComponentsInChildren<InteractableHost>(true);

            // Add all found rigidbodies to the result list
            List<GameObject> result = new();
            foreach (var rb in rbs)
            {
                result.Add(rb.gameObject);
            }

            // Add all non-static interactable hosts that don't have rigidbodies to the result list
            foreach (var host in hosts)
            {
                if (!host.IsStatic && !result.Has(host.gameObject))
                {
                    result.Add(host.gameObject);
                }
            }

            // Setup the arrays
            Length = result.Count;
            Items = new TempRigidbodyInfo[Length];

            for (var i = 0; i < Length; i++)
            {
                Items[i] = new TempRigidbodyInfo(result[i], result[i].transform, result[i].GetComponent<Rigidbody>());
            }
        }
    }
}
