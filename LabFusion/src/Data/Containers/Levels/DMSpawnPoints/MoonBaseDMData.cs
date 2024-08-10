using UnityEngine;

namespace LabFusion.Data
{
    public class MoonBaseDMData : DMLevelDataHandler
    {
        public override string LevelTitle => "09 - MoonBase";

        protected override Vector3[] DeathmatchSpawnPoints => new Vector3[23] {
            new (122.7655f, -5.5817f, -42.2308f),
            new (140.8222f, 5.8074f, -52.3688f),
            new (139.7279f, -3.9626f, -53.8363f),
            new (115.6281f, -8.1553f, 1.1752f),
            new (134.6004f, -5.4832f, 76.6579f),
            new (103.5483f, 49.8173f, 82.7182f),
            new (79.4005f, -4.7443f, 57.5612f),
            new (40.3474f, -9.6134f, 117.8718f),
            new (-26.1439f, -6.5776f, 135.3964f),
            new (-119.1221f, -6.5454f, 127.0153f),
            new (-80.3841f, -26.2927f, 30.9606f),
            new (-125.0571f, -5.9407f, -75.2538f),
            new (-142.5112f, -6.8284f, -24.5f),
            new (-56.206f, -6.3582f, -132.904f),
            new (-33.5513f, 18.2194f, -116.7787f),
            new (52.6826f, -16.1363f, -63.4711f),
            new (118.89f, -3.5999f, -98.8026f),
            new (67.9719f, -6.2497f, -5.6044f),
            new (-29.476f, -6.0705f, -101.6569f),
            new (66.9293f, 19.7641f, 92.2311f),
            new (114.0917f, 19.7641f, 46.3862f),
            new (-33.2483f, -26.3449f, 21.1142f),
            new (132.2135f, -8.9626f, -55.3668f),
        }; // 23 spawn points my god, what is this, tarkov?
    }
}
