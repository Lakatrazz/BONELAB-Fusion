using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SLZ.Interaction;

using UnityEngine;

namespace LabFusion.Syncables
{
    public class PropSyncable : ISyncable {
        public static readonly Dictionary<GameObject, PropSyncable> Cache = new Dictionary<GameObject, PropSyncable>();

        public readonly Grip[] PropGrips;
        public readonly Rigidbody[] Rigidbodies;

        public readonly GameObject GameObject;

        public ushort Id;

        public PropSyncable(GameObject go, Grip[] grips) {
            GameObject = go;

            PropGrips = grips;
            Rigidbodies = go.GetComponentsInChildren<Rigidbody>(true);

            Cache.Add(go, this);
        }

        public void Cleanup() {
            Cache.Remove(GameObject);
        }

        public Grip GetGrip(ushort index) {
            if (PropGrips != null && PropGrips.Length > index)
                return PropGrips[index];
            return null;
        }

        public void OnRegister(ushort id) {
            Id = id;
        }

        public ushort GetId() {
            return Id;
        }

        public byte? GetIndex(Grip grip)
        {
            for (byte i = 0; i < PropGrips.Length; i++)
            {
                if (PropGrips[i] == grip)
                    return i;
            }
            return null;
        }

        public bool IsQueued() {
            return SyncManager.QueuedSyncables.Contains(this);
        }
    }
}
