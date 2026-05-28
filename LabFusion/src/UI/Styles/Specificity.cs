namespace LabFusion.UI.Styles;

public struct Specificity : IEquatable<Specificity>, IComparable<Specificity>
{
    public static readonly Specificity Identity = new(0, 0, 0);

    public static readonly Specificity UnitID = new(1, 0, 0);

    public static readonly Specificity UnitClass = new(0, 1, 0);

    public static readonly Specificity UnitType = new(0, 0, 1);

    public int IDValue { get; set; }

    public int ClassValue { get; set; }

    public int TypeValue { get; set; }

    public Specificity(int idValue, int classValue, int typeValue)
    {
        IDValue = idValue;
        ClassValue = classValue;
        TypeValue = typeValue;
    }

    public readonly bool Equals(Specificity other)
    {
        return IDValue == other.IDValue && ClassValue == other.ClassValue && TypeValue == other.TypeValue;
    }

    public override readonly bool Equals(object obj)
    {
        return obj is Specificity specificity && Equals(specificity);
    }

    public override readonly int GetHashCode() => HashCode.Combine(IDValue, ClassValue, TypeValue);

    public readonly int CompareTo(Specificity other)
    {
        if (this > other)
        {
            return 1;
        }

        if (this < other)
        {
            return -1;
        }

        return 0;
    }

    public static Specificity operator +(Specificity left, Specificity right) => new(left.IDValue + right.IDValue, left.ClassValue + right.ClassValue, left.TypeValue + right.TypeValue);

    public static bool operator >(Specificity left, Specificity right)
    {
        if (left.IDValue != right.IDValue)
        {
            return left.IDValue > right.IDValue;
        }

        if (left.ClassValue != right.ClassValue)
        {
            return left.ClassValue > right.ClassValue;
        }

        return left.TypeValue > right.TypeValue;
    }

    public static bool operator <(Specificity left, Specificity right)
    {
        if (left.IDValue != right.IDValue)
        {
            return left.IDValue < right.IDValue;
        }

        if (left.ClassValue != right.ClassValue)
        {
            return left.ClassValue < right.ClassValue;
        }

        return left.TypeValue < right.TypeValue;
    }

    public static bool operator >=(Specificity left, Specificity right) => !(left < right);

    public static bool operator <=(Specificity left, Specificity right) => !(left > right);

    public static bool operator ==(Specificity left, Specificity right) => left.Equals(right);

    public static bool operator !=(Specificity left, Specificity right) => !(left == right);
}
