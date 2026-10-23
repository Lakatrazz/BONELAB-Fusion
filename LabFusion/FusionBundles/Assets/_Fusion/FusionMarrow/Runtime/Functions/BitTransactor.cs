using UnityEngine;

#if MELONLOADER
using MelonLoader;

using LabFusion.SDK.Points;
#endif

namespace LabFusion.Marrow.Integration
{
#if MELONLOADER
    [RegisterTypeInIl2Cpp]
#endif
    public class BitTransactor : MonoBehaviour
    {
#if MELONLOADER
        public BitTransactor(IntPtr intPtr) : base(intPtr) { }

#pragma warning disable CA1822 // Mark members as static
        public void GiveBits(int count)
        {
            PointItemManager.RewardBits(count);
        }

        public void TakeBits(int count)
        {
            PointItemManager.DecrementBits(count);
        }

        public int GetBitCount()
        {
            return PointItemManager.GetBitCount();
        }

        public bool HasBits(int count)
        {
            return count <= GetBitCount();
        }
#pragma warning restore CA1822 // Mark members as static

#else
        public void GiveBits(int count) { }

        public void TakeBits(int count) { }

        public int GetBitCount() { return 0; }

        public bool HasBits(int count) { return false; }
#endif
    }
}