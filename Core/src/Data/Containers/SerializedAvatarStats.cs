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
    public class SerializedAvatarStats : IFusionSerializable {
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
