using LabFusion.SDK.Scene;

using UnityEngine;

namespace LabFusion.Bonelab.Scene;

public class DungeonWarriorEventHandler : GamemodeLevelEventHandler
{
    public override string LevelBarcode => "c2534c5a-5c2f-4eef-a851-66214c657665";

    public override Vector3[] GamemodeMarkerPoints => new Vector3[] {
        new(54.2045f, -7.5652f, -65.2812f),
        new(62.5462f, 9.3181f, -77.1614f),
        new(49.9896f, 11.2435f, -75.2906f),
        new(62.4793f, 3.2703f, -75.1372f),
        new(58.0072f, 19.0727f, -71.7206f),
        new(54.3207f, -7.8262f, -73.7793f),
        new(58.6233f, 21.2966f, -74.2617f),
    };
}
