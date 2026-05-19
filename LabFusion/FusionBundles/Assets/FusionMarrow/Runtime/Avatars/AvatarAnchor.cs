using System;

namespace LabFusion.Marrow.Integration
{
    public struct AvatarAnchor : IEquatable<AvatarAnchor>
    {
        public AvatarPoint Point;

        public AvatarAlignment Alignment;

        public AvatarSide Side;

        public AvatarAnchor(AvatarPoint point) : this(point, AvatarAlignment.Center, AvatarSide.Center) { }

        public AvatarAnchor(AvatarPoint point, AvatarAlignment alignment) : this(point, alignment, AvatarSide.Center) { }

        public AvatarAnchor(AvatarPoint point, AvatarSide side) : this(point, AvatarAlignment.Center, side) { }

        public AvatarAnchor(AvatarPoint point, AvatarAlignment alignment, AvatarSide side)
        {
            Point = point;
            Alignment = alignment;
            Side = side;
        }

        public readonly bool Equals(AvatarAnchor other)
        {
            if (Point != other.Point)
            {
                return false;
            }

            bool alignmentSupported = AvatarPointSupport.CheckAlignmentSupported(Point);

            if (alignmentSupported && Alignment != other.Alignment)
            {
                return false;
            }

            bool sideSupported = AvatarPointSupport.CheckSideSupported(Point);

            if (sideSupported && Side != other.Side)
            {
                return false;
            }

            return true;
        }

        public override readonly bool Equals(object obj)
        {
            return obj is AvatarAnchor anchor && Equals(anchor);
        }

        public override readonly int GetHashCode()
        {
            var hashCode = new HashCode();

            hashCode.Add(Point);

            bool alignmentSupported = AvatarPointSupport.CheckAlignmentSupported(Point);

            if (alignmentSupported)
            {
                hashCode.Add(Alignment);
            }

            bool sideSupported = AvatarPointSupport.CheckSideSupported(Point);

            if (sideSupported)
            {
                hashCode.Add(Side);
            }

            return hashCode.ToHashCode();
        }

        public static bool operator ==(AvatarAnchor left, AvatarAnchor right) => left.Equals(right);

        public static bool operator !=(AvatarAnchor left, AvatarAnchor right) => !left.Equals(right);
    }
}
