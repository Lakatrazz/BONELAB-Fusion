using LabFusion.Syncables;

using System.Collections.Generic;

using UnityEngine;

namespace LabFusion.Extensions
{
    public class SyncableComparer : IEqualityComparer<ISyncable>, IEqualityComparer<ushort>
    {
        public bool Equals(ISyncable lft, ISyncable rht) => lft == rht;

        public bool Equals(ushort lft, ushort rht) => lft == rht;

        public int GetHashCode(ISyncable obj) => obj.GetHashCode();

        public int GetHashCode(ushort sh) => sh.GetHashCode();
    }
}

