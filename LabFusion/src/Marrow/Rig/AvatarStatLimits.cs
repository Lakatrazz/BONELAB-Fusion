namespace LabFusion.Marrow.Rig;

/// <summary>
/// Absolute limits set for avatar stats that will cause clients to be kicked if exceeded.
/// </summary>
public static class AvatarStatLimits
{
    public static readonly float MaxHeight = 30f;
    public static readonly float MinHeight = 0.01f;

    public static readonly float MaxArmLength = 20f;
    public static readonly float MinArmLength = -MaxArmLength;

    public static readonly float MaxLegLength = 20f;
    public static readonly float MinLegLength = -MaxLegLength;

    public static readonly float MaxStrengthUpper = 1000000f;
    public static readonly float MinStrengthUpper = -MaxStrengthUpper;

    public static readonly float MaxStrengthLower = 1000000f;
    public static readonly float MinStrengthLower = -MaxStrengthLower;

    public static readonly float MaxAgility = 100f;
    public static readonly float MinAgility = -MaxAgility;

    public static readonly float MaxSpeed = 1000f;
    public static readonly float MinSpeed = -MaxSpeed;

    public static readonly float MaxMass = 1000000f;
    public static readonly float MinMass = 0f;
}
