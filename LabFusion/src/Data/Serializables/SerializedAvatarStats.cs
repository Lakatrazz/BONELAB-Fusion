using LabFusion.Marrow.Serialization;
using LabFusion.Network.Serialization;

using UnityEngine;

using Avatar = Il2CppSLZ.VRMK.Avatar;

namespace LabFusion.Data;

// We manually send proportions, stats, and mass incase of not having an avatar
// And also stat modifier mods (Quicksilver, StatModifier, Spiderlab)
// This has a LOT of data, so this should ONLY be sent when necessary!
public class SerializedAvatarStats : INetSerializable
{
    public const int Size = sizeof(float) * 73 + SerializableSoftEllipse.Size * 8;

    public int? GetSize() => Size;

    // Root scale
    public Vector3 localScale;

    // Ellipse/offset values
    public float headTop;
    public float chinY;
    public float underbustY;
    public float waistY;
    public float highHipY;
    public float crotchBottom;

    public float headEllipseX;
    public float jawEllipseX;
    public float neckEllipseX;
    public float chestEllipseX;
    public float waistEllipseX;
    public float highHipsEllipseX;
    public float hipsEllipseX;

    public float headEllipseZ;
    public float jawEllipseZ;
    public float neckEllipseZ;
    public float sternumEllipseZ;
    public float chestEllipseZ;
    public float waistEllipseZ;
    public float highHipsEllipseZ;
    public float hipsEllipseZ;

    public float headEllipseNegZ;
    public float jawEllipseNegZ;
    public float neckEllipseNegZ;
    public float sternumEllipseNegZ;
    public float chestEllipseNegZ;
    public float waistEllipseNegZ;
    public float highHipsEllipseNegZ;
    public float hipsEllipseNegZ;

    // Soft ellipses
    public SerializableSoftEllipse thighUpperEllipse;
    public SerializableSoftEllipse kneeEllipse;
    public SerializableSoftEllipse calfEllipse;
    public SerializableSoftEllipse ankleEllipse;
    public SerializableSoftEllipse upperarmEllipse;
    public SerializableSoftEllipse elbowEllipse;
    public SerializableSoftEllipse forearmEllipse;
    public SerializableSoftEllipse wristEllipse;

    // Proportions
    public float eyeHeight;
    public float heightPercent;
    public float c1HeightPercent;
    public float height;
    public float t1HeightPercent;
    public float skullHeight;
    public float sacrumHeightPercent;
    public float chestHeight;
    public float chestToShoulderPerc;
    public float pelvisHeight;
    public float legUpperPercent;
    public float clavicleLength;
    public float armUpperPercent;
    public float legUpperLength;
    public float armUpperLength;
    public float armLowerPercent;
    public float legLowerPercent;
    public float armLowerLength;
    public float legLowerLength;
    public float carpalPercent;
    public float palmOffsetLength;
    public Vector3 sternumOffsetPercent;
    public float footLength;
    public float carpalLength;
    public float armPercent;
    public float shoulderToPalmPercent;
    public float armLength;

    // Stats
    public float agility;
    public float speed;
    public float strengthUpper;
    public float strengthLower;
    public float vitality;
    public float intelligence;

    // Mass
    public float massArm;
    public float massChest;
    public float massHead;
    public float massLeg;
    public float massPelvis;
    public float massTotal;

    public SerializedAvatarStats() { }

    public SerializedAvatarStats(Avatar avatar)
    {
        // Save the scale
        localScale = avatar.transform.localScale;

        // Save ellipse/offset values
        headTop = avatar._headTop;
        chinY = avatar._chinY;
        underbustY = avatar._underbustY;
        waistY = avatar._waistY;
        highHipY = avatar._highHipY;
        crotchBottom = avatar._crotchBottom;

        headEllipseX = avatar._headEllipseX;
        jawEllipseX = avatar._jawEllipseX;
        neckEllipseX = avatar._neckEllipseX;
        chestEllipseX = avatar._chestEllipseX;
        waistEllipseX = avatar._waistEllipseX;
        highHipsEllipseX = avatar.HighHipsEllipseX;
        hipsEllipseX = avatar._hipsEllipseX;

        headEllipseZ = avatar._headEllipseZ;
        jawEllipseZ = avatar._jawEllipseZ;
        neckEllipseZ = avatar._neckEllipseZ;
        sternumEllipseZ = avatar._sternumEllipseZ;
        chestEllipseZ = avatar._chestEllipseZ;
        waistEllipseZ = avatar._waistEllipseZ;
        highHipsEllipseZ = avatar._highHipsEllipseZ;
        hipsEllipseZ = avatar._hipsEllipseZ;

        headEllipseNegZ = avatar._headEllipseNegZ;
        jawEllipseNegZ = avatar._jawEllipseNegZ;
        neckEllipseNegZ = avatar._neckEllipseNegZ;
        sternumEllipseNegZ = avatar._sternumEllipseNegZ;
        chestEllipseNegZ = avatar._chestEllipseNegZ;
        waistEllipseNegZ = avatar._waistEllipseNegZ;
        highHipsEllipseNegZ = avatar._highHipsEllipseNegZ;
        hipsEllipseNegZ = avatar._hipsEllipseNegZ;

        // Save soft ellipses
        thighUpperEllipse = avatar._thighUpperEllipse;
        kneeEllipse = avatar._kneeEllipse;
        calfEllipse = avatar._calfEllipse;
        ankleEllipse = avatar._ankleEllipse;
        upperarmEllipse = avatar._upperarmEllipse;
        elbowEllipse = avatar._elbowEllipse;
        forearmEllipse = avatar._forearmEllipse;
        wristEllipse = avatar._wristEllipse;

        // Save proportions
        eyeHeight = avatar._eyeHeight;
        heightPercent = avatar._heightPercent;
        c1HeightPercent = avatar._c1HeightPercent;
        height = avatar._height;
        t1HeightPercent = avatar._t1HeightPercent;
        skullHeight = avatar._skullHeight;
        sacrumHeightPercent = avatar._sacrumHeightPercent;
        chestHeight = avatar._chestHeight;
        chestToShoulderPerc = avatar._chestToShoulderPerc;
        pelvisHeight = avatar._pelvisHeight;
        legUpperPercent = avatar._legUpperPercent;
        clavicleLength = avatar._clavicleLength;
        armUpperPercent = avatar._armUpperPercent;
        legUpperLength = avatar._legUpperLength;
        armUpperLength = avatar._armUpperLength;
        armLowerPercent = avatar._armLowerPercent;
        legLowerPercent = avatar._legLowerPercent;
        armLowerLength = avatar._armLowerLength;
        legLowerLength = avatar._legLowerLength;
        carpalPercent = avatar._carpalPercent;
        palmOffsetLength = avatar._palmOffsetLength;
        sternumOffsetPercent = avatar._sternumOffsetPercent;
        footLength = avatar._footLength;
        carpalLength = avatar._carpalLength;
        armPercent = avatar._armPercent;
        shoulderToPalmPercent = avatar._shoulderToPalmPercent;
        armLength = avatar._armLength;

        // Save stats
        agility = avatar._agility;
        speed = avatar._speed;
        strengthUpper = avatar._strengthUpper;
        strengthLower = avatar._strengthLower;
        vitality = avatar._vitality;
        intelligence = avatar._intelligence;

        // Save mass
        massArm = avatar._massArm;
        massChest = avatar._massChest;
        massHead = avatar._massHead;
        massLeg = avatar._massLeg;
        massPelvis = avatar._massPelvis;
        massTotal = avatar._massTotal;
    }

    public void CopyTo(Avatar avatar)
    {
        // Copy ellipse/offset values
        avatar._headTop = headTop;
        avatar._chinY = chinY;
        avatar._underbustY = underbustY;
        avatar._waistY = waistY;
        avatar._highHipY = highHipY;
        avatar._crotchBottom = crotchBottom;

        avatar._headEllipseX = headEllipseX;
        avatar._jawEllipseX = jawEllipseX;
        avatar._neckEllipseX = neckEllipseX;
        avatar._chestEllipseX = chestEllipseX;
        avatar._waistEllipseX = waistEllipseX;
        avatar._highHipsEllipseX = highHipsEllipseX;
        avatar._hipsEllipseX = hipsEllipseX;

        avatar._headEllipseZ = headEllipseZ;
        avatar._jawEllipseZ = jawEllipseZ;
        avatar._neckEllipseZ = neckEllipseZ;
        avatar._sternumEllipseZ = sternumEllipseZ;
        avatar._chestEllipseZ = chestEllipseZ;
        avatar._waistEllipseZ = waistEllipseZ;
        avatar._highHipsEllipseZ = highHipsEllipseZ;
        avatar._hipsEllipseZ = hipsEllipseZ;

        avatar._headEllipseNegZ = headEllipseNegZ;
        avatar._jawEllipseNegZ = jawEllipseNegZ;
        avatar._neckEllipseNegZ = neckEllipseNegZ;
        avatar._sternumEllipseNegZ = sternumEllipseNegZ;
        avatar._chestEllipseNegZ = chestEllipseNegZ;
        avatar._waistEllipseNegZ = waistEllipseNegZ;
        avatar._highHipsEllipseNegZ = highHipsEllipseNegZ;
        avatar._hipsEllipseNegZ = hipsEllipseNegZ;

        // Copy soft ellipses
        avatar._thighUpperEllipse = thighUpperEllipse;
        avatar._kneeEllipse = kneeEllipse;
        avatar._calfEllipse = calfEllipse;
        avatar._ankleEllipse = ankleEllipse;
        avatar._upperarmEllipse = upperarmEllipse;
        avatar._elbowEllipse = elbowEllipse;
        avatar._forearmEllipse = forearmEllipse;
        avatar._wristEllipse = wristEllipse;

        // Copy proportions
        avatar._eyeHeight = eyeHeight;
        avatar._heightPercent = heightPercent;
        avatar._c1HeightPercent = c1HeightPercent;
        avatar._height = height;
        avatar._t1HeightPercent = t1HeightPercent;
        avatar._skullHeight = skullHeight;
        avatar._sacrumHeightPercent = sacrumHeightPercent;
        avatar._chestHeight = chestHeight;
        avatar._chestToShoulderPerc = chestToShoulderPerc;
        avatar._pelvisHeight = pelvisHeight;
        avatar._legUpperPercent = legUpperPercent;
        avatar._clavicleLength = clavicleLength;
        avatar._armUpperPercent = armUpperPercent;
        avatar._legUpperLength = legUpperLength;
        avatar._armUpperLength = armUpperLength;
        avatar._armLowerPercent = armLowerPercent;
        avatar._legLowerPercent = legLowerPercent;
        avatar._armLowerLength = armLowerLength;
        avatar._legLowerLength = legLowerLength;
        avatar._carpalPercent = carpalPercent;
        avatar._palmOffsetLength = palmOffsetLength;
        avatar._sternumOffsetPercent = sternumOffsetPercent;
        avatar._footLength = footLength;
        avatar._carpalLength = carpalLength;
        avatar._armPercent = armPercent;
        avatar._shoulderToPalmPercent = shoulderToPalmPercent;
        avatar._armLength = armLength;

        // Copy stats
        avatar._agility = agility;
        avatar._speed = speed;
        avatar._strengthUpper = strengthUpper;
        avatar._strengthLower = strengthLower;
        avatar._vitality = vitality;
        avatar._intelligence = intelligence;

        // Copy mass
        avatar._massArm = massArm;
        avatar._massChest = massChest;
        avatar._massHead = massHead;
        avatar._massLeg = massLeg;
        avatar._massPelvis = massPelvis;
        avatar._massTotal = massTotal;
    }

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref localScale);

        serializer.SerializeValue(ref headTop);
        serializer.SerializeValue(ref chinY);
        serializer.SerializeValue(ref underbustY);
        serializer.SerializeValue(ref waistY);
        serializer.SerializeValue(ref highHipY);
        serializer.SerializeValue(ref crotchBottom);

        serializer.SerializeValue(ref headEllipseX);
        serializer.SerializeValue(ref jawEllipseX);
        serializer.SerializeValue(ref neckEllipseX);
        serializer.SerializeValue(ref chestEllipseX);
        serializer.SerializeValue(ref waistEllipseX);
        serializer.SerializeValue(ref highHipsEllipseX);
        serializer.SerializeValue(ref hipsEllipseX);

        serializer.SerializeValue(ref headEllipseZ);
        serializer.SerializeValue(ref jawEllipseZ);
        serializer.SerializeValue(ref neckEllipseZ);
        serializer.SerializeValue(ref sternumEllipseZ);
        serializer.SerializeValue(ref chestEllipseZ);
        serializer.SerializeValue(ref waistEllipseZ);
        serializer.SerializeValue(ref highHipsEllipseZ);
        serializer.SerializeValue(ref hipsEllipseZ);

        serializer.SerializeValue(ref headEllipseNegZ);
        serializer.SerializeValue(ref jawEllipseNegZ);
        serializer.SerializeValue(ref neckEllipseNegZ);
        serializer.SerializeValue(ref sternumEllipseNegZ);
        serializer.SerializeValue(ref chestEllipseNegZ);
        serializer.SerializeValue(ref waistEllipseNegZ);
        serializer.SerializeValue(ref highHipsEllipseNegZ);
        serializer.SerializeValue(ref hipsEllipseNegZ);

        serializer.SerializeValue(ref thighUpperEllipse);
        serializer.SerializeValue(ref kneeEllipse);
        serializer.SerializeValue(ref calfEllipse);
        serializer.SerializeValue(ref ankleEllipse);
        serializer.SerializeValue(ref upperarmEllipse);
        serializer.SerializeValue(ref elbowEllipse);
        serializer.SerializeValue(ref forearmEllipse);
        serializer.SerializeValue(ref wristEllipse);

        serializer.SerializeValue(ref eyeHeight);
        serializer.SerializeValue(ref heightPercent);
        serializer.SerializeValue(ref c1HeightPercent);
        serializer.SerializeValue(ref height);
        serializer.SerializeValue(ref t1HeightPercent);
        serializer.SerializeValue(ref skullHeight);
        serializer.SerializeValue(ref sacrumHeightPercent);
        serializer.SerializeValue(ref chestHeight);
        serializer.SerializeValue(ref chestToShoulderPerc);
        serializer.SerializeValue(ref pelvisHeight);
        serializer.SerializeValue(ref legUpperPercent);
        serializer.SerializeValue(ref clavicleLength);
        serializer.SerializeValue(ref armUpperPercent);
        serializer.SerializeValue(ref legUpperLength);
        serializer.SerializeValue(ref armUpperLength);
        serializer.SerializeValue(ref armLowerPercent);
        serializer.SerializeValue(ref legLowerPercent);
        serializer.SerializeValue(ref armLowerLength);
        serializer.SerializeValue(ref legLowerLength);
        serializer.SerializeValue(ref carpalPercent);
        serializer.SerializeValue(ref palmOffsetLength);
        serializer.SerializeValue(ref sternumOffsetPercent);
        serializer.SerializeValue(ref footLength);
        serializer.SerializeValue(ref carpalLength);
        serializer.SerializeValue(ref armPercent);
        serializer.SerializeValue(ref shoulderToPalmPercent);
        serializer.SerializeValue(ref armLength);

        serializer.SerializeValue(ref agility);
        serializer.SerializeValue(ref speed);
        serializer.SerializeValue(ref strengthUpper);
        serializer.SerializeValue(ref strengthLower);
        serializer.SerializeValue(ref vitality);
        serializer.SerializeValue(ref intelligence);

        serializer.SerializeValue(ref massArm);
        serializer.SerializeValue(ref massChest);
        serializer.SerializeValue(ref massHead);
        serializer.SerializeValue(ref massLeg);
        serializer.SerializeValue(ref massPelvis);
        serializer.SerializeValue(ref massTotal);
    }
}