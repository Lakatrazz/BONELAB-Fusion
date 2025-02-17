using LabFusion.Menu;
using LabFusion.Menu.Data;
using LabFusion.Player;

using UnityEngine;

namespace LabFusion.SDK.Gamemodes;

public class SmashBones : Gamemode
{
    public override string Title => "Smash Bones";

    public override string Author => FusionMod.ModAuthor;

    public override string Description =>
        "Attack other players to build up damage, and knock them off the map to gain points! " +
        "Use randomly spawned items to gain advantages over your opponents! " +
        "Requires a Smash Bones supported map.";

    public override Texture Logo => MenuResources.GetGamemodeIcon(Title);

    public override bool AutoHolsterOnDeath => true;

    public override bool DisableDevTools => true;

    public override bool DisableSpawnGun => true;

    public override bool DisableManualUnragdoll => true;

    private int _minimumPlayers = 2;
    public int MinimumPlayers
    {
        get
        {
            return _minimumPlayers;
        }
        set
        {
            _minimumPlayers = value;

            if (!IsStarted && IsSelected)
            {
                GamemodeManager.ValidateReadyConditions();
            }
        }
    }

    public override GroupElementData CreateSettingsGroup()
    {
        var group = base.CreateSettingsGroup();

        var generalGroup = new GroupElementData("General");

        group.AddElement(generalGroup);

        var minimumPlayersData = new IntElementData()
        {
            Title = "Minimum Players",
            Value = MinimumPlayers,
            Increment = 1,
            MinValue = 1,
            MaxValue = 255,
            OnValueChanged = (v) =>
            {
                MinimumPlayers = v;
            }
        };

        generalGroup.AddElement(minimumPlayersData);

        return group;
    }

    public override bool CheckReadyConditions()
    {
        return PlayerIdManager.PlayerCount >= MinimumPlayers;
    }
}
