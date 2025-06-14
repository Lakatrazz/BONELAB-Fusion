using UnityEngine;

using LabFusion.Marrow.Scene;
using LabFusion.Marrow;
using LabFusion.SDK.Gamemodes;

namespace LabFusion.SDK.Scene;

/// <summary>
/// A version of <see cref="LevelEventHandler"/> that automatically creates GamemodeMarkers in the scene.
/// </summary>
public abstract class GamemodeLevelEventHandler : LevelEventHandler
{
    /// <summary>
    /// The points to create GamemodeMarkers at.
    /// </summary>
    public abstract Vector3[] GamemodeMarkerPoints { get; }


    protected override void OnLevelLoaded()
    {
        GamemodeHelper.CreateMarkers(GamemodeMarkerPoints, null, FusionBoneTagReferences.TeamLavaGangReference.Barcode.ID, FusionBoneTagReferences.TeamSabrelakeReference.Barcode.ID);
    }
}
