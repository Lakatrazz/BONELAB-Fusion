using LabFusion.MarrowIntegration;
using LabFusion.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.Data {
    public class MoonBaseDMData : DMLevelDataHandler {
        public override string LevelTitle => "09- MoonBase";

        protected override Vector3[] DeathmatchSpawnPoints => new Vector3[23] {
    new Vector3(122.7655f, -5.5817f, -42.2308f),
    new Vector3(140.8222f, 5.8074f, -52.3688f),
    new Vector3(139.7279f, -3.9626f, -53.8363f),
    new Vector3(115.6281f, -8.1553f, 1.1752f),
    new Vector3(134.6004f, -5.4832f, 76.6579f),
    new Vector3(103.5483f, 49.8173f, 82.7182f),
    new Vector3(79.4005f, -4.7443f, 57.5612f),
    new Vector3(40.3474f, -9.6134f, 117.8718f),
    new Vector3(-26.1439f, -6.5776f, 135.3964f),
    new Vector3(-119.1221f, -6.5454f, 127.0153f),
    new Vector3(-80.3841f, -26.2927f, 30.9606f),
    new Vector3(-125.0571f, -5.9407f, -75.2538f),
    new Vector3(-142.5112f, -6.8284f, -24.5f),
    new Vector3(-56.206f, -6.3582f, -132.904f),
    new Vector3(-33.5513f, 18.2194f, -116.7787f),
    new Vector3(52.6826f, -16.1363f, -63.4711f),
    new Vector3(118.89f, -3.5999f, -98.8026f),
    new Vector3(67.9719f, -6.2497f, -5.6044f),
    new Vector3(-29.476f, -6.0705f, -101.6569f),
    new Vector3(66.9293f, 19.7641f, 92.2311f),
    new Vector3(114.0917f, 19.7641f, 46.3862f),
    new Vector3(-33.2483f, -26.3449f, 21.1142f),
    new Vector3(132.2135f, -8.9626f, -55.3668f),
        }; // 23 spawn points my god, what is this, tarkov?
    }
}
