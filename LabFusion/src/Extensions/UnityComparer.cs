using Object = UnityEngine.Object;

namespace LabFusion.Extensions
{
    public class UnityComparer : IEqualityComparer<Object>
    {
        public bool Equals(Object lft, Object rht)
        {
            return lft == rht;
        }

        public int GetHashCode(Object obj)
        {
            return obj.GetHashCode();
        }
    }
}

