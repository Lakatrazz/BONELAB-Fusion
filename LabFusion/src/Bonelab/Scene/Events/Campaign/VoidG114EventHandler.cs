using UnityEngine;

using LabFusion.UI;
using LabFusion.SDK.Points;
using LabFusion.SDK.Scene;

namespace LabFusion.Bonelab.Scene;

public class VoidG114EventHandler : GamemodeLevelEventHandler
{
    public override string LevelBarcode => "fa534c5a868247138f50c62e424c4144.Level.VoidG114";

    public override Vector3[] GamemodeMarkerPoints => new Vector3[] {
        new(28.0497f, -0.2327f, 0.0443f),
        new(27.741f, 4.8885f, -5.717f),
        new(27.5797f, 4.6144f, -6.9054f),
        new(28.0605f, -0.1453f, 4.4828f),
        new(39.0173f, -0.7884f, 15.841f),
        new(13.4692f, -0.8171f, 9.5978f),
        new(18.9537f, -0.8847f, -10.8288f),
        new(35.9514f, -0.8819f, -12.1057f),
    };

    // Point shop setup
    public static readonly Vector3 PointShopPosition = new(25.4318f, -1.1765f, 1.8907f);
    public static readonly Quaternion PointShopRotation = Quaternion.Euler(0f, 90f, 0f);

    public static readonly Vector3 SupportCubePosition = new(25.5273f, -1.6745f, 2.1946f);
    public static readonly Quaternion SupportCubeRotation = Quaternion.Euler(0f, 0f, 0f);
    public static readonly Vector3 SupportCubeScale = new(2.04f, 1f, 2.74f);

    // Info box setup
    public static readonly Vector3 InfoBoxPosition = new(29.0255f, -2.01f, 11.9655f);
    public static readonly Quaternion InfoBoxRotation = Quaternion.Euler(0f, 180f, 0f);

    // Cup board setup
    public static readonly Vector3 CupBoardPosition = new(26.5255f, -2.01f, 11.9655f);
    public static readonly Quaternion CupBoardRotation = Quaternion.Euler(0f, 180f, 0f);

    protected override void OnLevelLoaded()
    {
        base.OnLevelLoaded();

        CreateSupportCube();

        // Point shop
        PointShopHelper.SpawnBitMart(PointShopPosition, PointShopRotation);

        // Info box
        InfoBoxHelper.SpawnInfoBoard(InfoBoxPosition, InfoBoxRotation);

        // Cup board
        CupBoardHelper.SpawnAchievementBoard(CupBoardPosition, CupBoardRotation);
    }

    private static void CreateSupportCube()
    {
        GameObject supportCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        supportCube.name = "BitMart Support Cube";
        supportCube.transform.position = SupportCubePosition;
        supportCube.transform.rotation = SupportCubeRotation;
        supportCube.transform.localScale = SupportCubeScale;

        var meshRenderer = supportCube.GetComponentInChildren<MeshRenderer>();
        var planeRenderer = GameObject.Find("plane_1x6").GetComponentInChildren<MeshRenderer>();

        meshRenderer.material = planeRenderer.material;
    }
}