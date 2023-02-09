using LabFusion.Data;
using LabFusion.Network;

using SLZ.VRMK;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

using Avatar = SLZ.VRMK.Avatar;

namespace LabFusion.Data {
    // We manually send proportions, stats, and mass incase of not having an avatar
    // And also stat modifier mods (Quicksilver, StatModifier, Spiderlab)
    // This has a LOT of data, so this should ONLY be sent when necessary!
    public class SerializedAvatarStats : IFusionSerializable {
        public const int Size = sizeof(float) * 70 + SerializedSoftEllipse.Size * 8;

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
        public SerializedSoftEllipse thighUpperEllipse;
        public SerializedSoftEllipse kneeEllipse;
        public SerializedSoftEllipse calfEllipse;
        public SerializedSoftEllipse ankleEllipse;
        public SerializedSoftEllipse upperarmEllipse;
        public SerializedSoftEllipse elbowEllipse;
        public SerializedSoftEllipse forearmEllipse;
        public SerializedSoftEllipse wristEllipse;

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

        public SerializedAvatarStats(Avatar avatar) {
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
            vitality = avatar._strengthLower;
            intelligence = avatar._intelligence;

            // Save mass
            massArm = avatar._massArm;
            massChest = avatar._massChest;
            massHead = avatar._massHead;
            massLeg = avatar._massLeg;
            massPelvis = avatar._massPelvis;
            massTotal = avatar._massTotal;
        }

        public void CopyTo(Avatar avatar) {
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

        public void Serialize(FusionWriter writer) {
            // Write ellipse/offset values
            writer.Write(headTop);
            writer.Write(chinY);
            writer.Write(underbustY);
            writer.Write(waistY);
            writer.Write(highHipY);
            writer.Write(crotchBottom);
            
            writer.Write(headEllipseX);
            writer.Write(jawEllipseX);
            writer.Write(neckEllipseX);
            writer.Write(chestEllipseX);
            writer.Write(waistEllipseX);
            writer.Write(highHipsEllipseX);
            writer.Write(hipsEllipseX);
            
            writer.Write(headEllipseZ);
            writer.Write(jawEllipseZ);
            writer.Write(neckEllipseZ);
            writer.Write(sternumEllipseZ);
            writer.Write(chestEllipseZ);
            writer.Write(waistEllipseZ);
            writer.Write(highHipsEllipseZ);
            writer.Write(hipsEllipseZ);

            writer.Write(headEllipseNegZ);
            writer.Write(jawEllipseNegZ);
            writer.Write(neckEllipseNegZ);
            writer.Write(sternumEllipseNegZ);
            writer.Write(chestEllipseNegZ);
            writer.Write(waistEllipseNegZ);
            writer.Write(highHipsEllipseNegZ);
            writer.Write(hipsEllipseNegZ);

            // Write soft ellipses
            writer.Write(thighUpperEllipse);
            writer.Write(kneeEllipse);
            writer.Write(calfEllipse);
            writer.Write(ankleEllipse);
            writer.Write(upperarmEllipse);
            writer.Write(elbowEllipse);
            writer.Write(forearmEllipse);
            writer.Write(wristEllipse);

            // Write proportions
            writer.Write(eyeHeight);
            writer.Write(heightPercent);
            writer.Write(c1HeightPercent);
            writer.Write(height);
            writer.Write(t1HeightPercent);
            writer.Write(skullHeight);
            writer.Write(sacrumHeightPercent);
            writer.Write(chestHeight);
            writer.Write(chestToShoulderPerc);
            writer.Write(pelvisHeight);
            writer.Write(legUpperPercent);
            writer.Write(clavicleLength);
            writer.Write(armUpperPercent);
            writer.Write(legUpperLength);
            writer.Write(armUpperLength);
            writer.Write(armLowerPercent);
            writer.Write(legLowerPercent);
            writer.Write(armLowerLength);
            writer.Write(legLowerLength);
            writer.Write(carpalPercent);
            writer.Write(palmOffsetLength);
            writer.Write(sternumOffsetPercent);
            writer.Write(footLength);
            writer.Write(carpalLength);
            writer.Write(armPercent);
            writer.Write(shoulderToPalmPercent);
            writer.Write(armLength);

            // Write stats
            writer.Write(agility);
            writer.Write(speed);
            writer.Write(strengthUpper);
            writer.Write(strengthLower);
            writer.Write(vitality);
            writer.Write(intelligence);

            // Write mass
            writer.Write(massArm);
            writer.Write(massChest);
            writer.Write(massHead);
            writer.Write(massLeg);
            writer.Write(massPelvis);
            writer.Write(massTotal);
        }

        public void Deserialize(FusionReader reader) {
            // Read ellipse/offset values
            headTop = reader.ReadSingle();
            chinY = reader.ReadSingle();
            underbustY = reader.ReadSingle();
            waistY = reader.ReadSingle();
            highHipY = reader.ReadSingle();
            crotchBottom = reader.ReadSingle();

            headEllipseX = reader.ReadSingle();
            jawEllipseX = reader.ReadSingle();
            neckEllipseX = reader.ReadSingle();
            chestEllipseX = reader.ReadSingle();
            waistEllipseX = reader.ReadSingle();
            highHipsEllipseX = reader.ReadSingle();
            hipsEllipseX = reader.ReadSingle();

            headEllipseZ = reader.ReadSingle();
            jawEllipseZ = reader.ReadSingle();
            neckEllipseZ = reader.ReadSingle();
            sternumEllipseZ = reader.ReadSingle();
            chestEllipseZ = reader.ReadSingle();
            waistEllipseZ = reader.ReadSingle();
            highHipsEllipseZ = reader.ReadSingle();
            hipsEllipseZ = reader.ReadSingle();

            headEllipseNegZ = reader.ReadSingle();
            jawEllipseNegZ = reader.ReadSingle();
            neckEllipseNegZ = reader.ReadSingle();
            sternumEllipseNegZ = reader.ReadSingle();
            chestEllipseNegZ = reader.ReadSingle();
            waistEllipseNegZ = reader.ReadSingle();
            highHipsEllipseNegZ = reader.ReadSingle();
            hipsEllipseNegZ = reader.ReadSingle();

            // Read soft ellipses
            thighUpperEllipse = reader.ReadFusionSerializable<SerializedSoftEllipse>();
            kneeEllipse = reader.ReadFusionSerializable<SerializedSoftEllipse>();
            calfEllipse = reader.ReadFusionSerializable<SerializedSoftEllipse>();
            ankleEllipse = reader.ReadFusionSerializable<SerializedSoftEllipse>();
            upperarmEllipse = reader.ReadFusionSerializable<SerializedSoftEllipse>();
            elbowEllipse = reader.ReadFusionSerializable<SerializedSoftEllipse>();
            forearmEllipse = reader.ReadFusionSerializable<SerializedSoftEllipse>();
            wristEllipse = reader.ReadFusionSerializable<SerializedSoftEllipse>();

            // Read proportions
            eyeHeight = reader.ReadSingle();
            heightPercent = reader.ReadSingle();
            c1HeightPercent = reader.ReadSingle();
            height = reader.ReadSingle();
            t1HeightPercent = reader.ReadSingle();
            skullHeight = reader.ReadSingle();
            sacrumHeightPercent = reader.ReadSingle();
            chestHeight = reader.ReadSingle();
            chestToShoulderPerc = reader.ReadSingle();
            pelvisHeight = reader.ReadSingle();
            legUpperPercent = reader.ReadSingle();
            clavicleLength = reader.ReadSingle();
            armUpperPercent = reader.ReadSingle();
            legUpperLength = reader.ReadSingle();
            armUpperLength = reader.ReadSingle();
            armLowerPercent = reader.ReadSingle();
            legLowerPercent = reader.ReadSingle();
            armLowerLength = reader.ReadSingle();
            legLowerLength = reader.ReadSingle();
            carpalPercent = reader.ReadSingle();
            palmOffsetLength = reader.ReadSingle();
            sternumOffsetPercent = reader.ReadVector3();
            footLength = reader.ReadSingle();
            carpalLength = reader.ReadSingle();
            armPercent = reader.ReadSingle();
            shoulderToPalmPercent = reader.ReadSingle();
            armLength = reader.ReadSingle();

            // Read stats
            agility = reader.ReadSingle();
            speed = reader.ReadSingle();
            strengthUpper = reader.ReadSingle();
            strengthLower = reader.ReadSingle();
            vitality = reader.ReadSingle();
            intelligence = reader.ReadSingle();

            // Read mass
            massArm = reader.ReadSingle();
            massChest = reader.ReadSingle();
            massHead = reader.ReadSingle();
            massLeg = reader.ReadSingle();
            massPelvis = reader.ReadSingle();
            massTotal = reader.ReadSingle();
        }
    }
}
