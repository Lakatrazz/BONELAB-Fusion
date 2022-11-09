using System.Collections.Generic;

using UnityEngine;

namespace LabFusion.Extensions
{
    public class UnityComparer : IEqualityComparer<Object>, IEqualityComparer<ushort>
    {
        public bool Equals(Object lft, Object rht) => lft == rht;

        public bool Equals(ushort lft, ushort rht) => lft == rht;

        public int GetHashCode(Object obj) => !obj.IsNOC() ? obj.GetHashCode() : -1;

        public int GetHashCode(ushort sh) => sh.GetHashCode();
    }
}

