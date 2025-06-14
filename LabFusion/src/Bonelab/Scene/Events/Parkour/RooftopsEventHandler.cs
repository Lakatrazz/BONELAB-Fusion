using LabFusion.SDK.Scene;

using UnityEngine;

namespace LabFusion.Bonelab.Scene;

public class RooftopsEventHandler : GamemodeLevelEventHandler
{
    public override string LevelBarcode => "c2534c5a-c6ac-48b4-9c5f-b5cd5363656e";

    public override Vector3[] GamemodeMarkerPoints => new Vector3[] {
        new(59.4848f, 93.0314f, -54.4698f),
        new(74.7662f, 90.0374f, -57.4251f),
        new(60.3601f, 92.2162f, -69.01f),
        new(53.8805f, 90.197f, -66.1647f),
        new(42.8871f, 90.197f, -66.5409f),
        new(35.5215f, 90.7248f, -60.718f),
        new(51.6421f, 93.698f, -55.6575f),
        new(45.2663f, 79.6913f, -38.3841f),
        new(59.7499f, 90.0374f, -53.5572f),
        new(42.109f, 93.569f, -66.7509f),
        new(37.153f, 90.0374f, -54.4797f),
        new(37.6532f, 90.197f, -66.376f),
        new(64.6106f, 93.5873f, -62.8519f),
        new(69.8322f, 99.0641f, -68.1835f),
    };
}
