using LabFusion.Marrow.Rig;
using LabFusion.Network.Serialization;

using UnityEngine;

using Avatar = Il2CppSLZ.VRMK.Avatar;

namespace LabFusion.Marrow.Data;

public class SerializedAvatarStats : INetSerializable
{
    public const int Size = sizeof(float) * 9;

    public int? GetSize() => Size;

    public float Height;

    public float ArmLength;

    public float LegLength;

    public float MassCumulative;

    public float Agility;

    public float Speed;

    public float StrengthUpper;

    public float StrengthLower;

    public float Vitality;

    public SerializedAvatarStats() { }

    public SerializedAvatarStats(Avatar avatar)
    {
        Height = GetHeight(avatar);
        ArmLength = GetArmLength(avatar);
        LegLength = GetLegLength(avatar);

        MassCumulative = GetMassCumulative(avatar);

        Agility = GetAgility(avatar);
        Speed = GetSpeed(avatar);
        StrengthUpper = GetStrengthUpper(avatar);
        StrengthLower = GetStrengthLower(avatar);
        Vitality = GetVitality(avatar);
    }

    public void CopyTo(Avatar avatar)
    {
        float armLengthMultiplier = ArmLength / GetArmLength(avatar);

        if (!Mathf.Approximately(armLengthMultiplier, 1f))
        {
            avatar._armLength *= armLengthMultiplier;
            avatar._armUpperLength *= armLengthMultiplier;
            avatar._armLowerLength *= armLengthMultiplier;
        }

        float legLengthMultiplier = LegLength / GetLegLength(avatar);

        if (!Mathf.Approximately(legLengthMultiplier, 1f)) 
        {
            avatar._legUpperLength *= legLengthMultiplier;
            avatar._legLowerLength *= legLengthMultiplier;
        }

        float massMultiplier = MassCumulative / GetMassCumulative(avatar);

        if (!Mathf.Approximately(massMultiplier, 1f))
        {
            avatar._massArm *= massMultiplier;
            avatar._massChest *= massMultiplier;
            avatar._massHead *= massMultiplier;
            avatar._massLeg *= massMultiplier;
            avatar._massPelvis *= massMultiplier;
            avatar._massTotal *= massMultiplier;
        }

        avatar._agility = Agility;
        avatar._speed = Speed;
        avatar._strengthUpper = StrengthUpper;
        avatar._strengthLower = StrengthLower;
        avatar._vitality = Vitality;
    }

    public bool IsValid()
    {
        if (!ValidateStat(Height, AvatarStatLimits.MinHeight, AvatarStatLimits.MaxHeight))
        {
            return false;
        }

        if (!ValidateStat(ArmLength, AvatarStatLimits.MinArmLength, AvatarStatLimits.MaxArmLength))
        {
            return false;
        }

        if (!ValidateStat(LegLength, AvatarStatLimits.MinLegLength, AvatarStatLimits.MaxLegLength))
        {
            return false;
        }

        if (!ValidateStat(MassCumulative, AvatarStatLimits.MinMass, AvatarStatLimits.MaxMass))
        {
            return false;
        }

        if (!ValidateStat(Agility, AvatarStatLimits.MinAgility, AvatarStatLimits.MaxAgility))
        {
            return false;
        }

        if (!ValidateStat(Speed, AvatarStatLimits.MinSpeed, AvatarStatLimits.MaxSpeed))
        {
            return false;
        }

        if (!ValidateStat(StrengthUpper, AvatarStatLimits.MinStrengthUpper, AvatarStatLimits.MaxStrengthLower))
        {
            return false;
        }

        if (!ValidateStat(StrengthLower, AvatarStatLimits.MinStrengthLower, AvatarStatLimits.MaxStrengthLower))
        {
            return false;
        }

        return true;
    }

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref Height);
        serializer.SerializeValue(ref ArmLength);
        serializer.SerializeValue(ref LegLength);

        serializer.SerializeValue(ref MassCumulative);

        serializer.SerializeValue(ref Agility);
        serializer.SerializeValue(ref Speed);
        serializer.SerializeValue(ref StrengthUpper);
        serializer.SerializeValue(ref StrengthLower);
        serializer.SerializeValue(ref Vitality);
    }

    private static float GetHeight(Avatar avatar) => avatar.height;

    private static float GetArmLength(Avatar avatar) => avatar.armLength;

    private static float GetLegLength(Avatar avatar) => avatar.legLowerLength + avatar.legUpperLength;

    private static float GetMassCumulative(Avatar avatar) => avatar.massArm * 2f + avatar.massChest + avatar.massHead + avatar.massLeg * 2f + avatar.massPelvis;

    private static float GetAgility(Avatar avatar) => avatar.agility;

    private static float GetSpeed(Avatar avatar) => avatar.speed;

    private static float GetStrengthUpper(Avatar avatar) => avatar.strengthUpper;

    private static float GetStrengthLower(Avatar avatar) => avatar.strengthLower;

    private static float GetVitality(Avatar avatar) => avatar.vitality;

    private static bool ValidateStat(float value, float min, float max)
    {
        if (float.IsNaN(value))
        {
            return false;
        }

        if (value < min)
        {
            return false;
        }

        if (value > max)
        {
            return false;
        }

        return true;
    }
}