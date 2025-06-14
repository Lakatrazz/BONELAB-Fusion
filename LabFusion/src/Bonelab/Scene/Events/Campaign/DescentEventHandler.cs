using LabFusion.Network;
using LabFusion.SDK.Achievements;
using LabFusion.Utilities;
using LabFusion.Player;
using LabFusion.Bonelab.Messages;
using LabFusion.SDK.Scene;

using Il2CppSLZ.Bonelab;
using Il2CppSLZ.Marrow;

using UnityEngine;

namespace LabFusion.Bonelab.Scene;

public struct DescentIntroEvent
{
    public int SelectionNumber;
    public DescentIntroType Type;

    public DescentIntroEvent(int selectionNumber, DescentIntroType type)
    {
        this.SelectionNumber = selectionNumber;
        this.Type = type;
    }
}

public struct DescentElevatorEvent
{
    public DescentElevatorType Type;

    public DescentElevatorEvent(DescentElevatorType type)
    {
        this.Type = type;
    }
}

public struct DescentNooseEvent
{
    public byte PlayerId;
    public DescentNooseType Type;

    public DescentNooseEvent(byte playerId, DescentNooseType type)
    {
        this.PlayerId = playerId;
        this.Type = type;
    }
}

public class DescentEventHandler : GamemodeLevelEventHandler
{
    public override string LevelBarcode => "c2534c5a-4197-4879-8cd3-4a695363656e";

    public override Vector3[] GamemodeMarkerPoints => new Vector3[] {
        new(123.9262f, -62.2602f, 166.4684f),
        new(125.4921f, -72.8109f, 186.0882f),
        new(122.0129f, -65.7702f, 192.8663f),
        new(107.1931f, -62.2902f, 191.8245f),
        new(87.1881f, -27.7636f, 184.0406f),
        new(135.6338f, -72.2126f, 210.5421f),
        new(123.1387f, -65.8102f, 216.6858f),
        new(107.2842f, -62.2902f, 214.4001f),
        new(107.0591f, -65.8102f, 173.6062f),
        new(158.7357f, -68.2125f, 210.362f),
        new(115.0106f, -72.8502f, 162.9939f),
        new(123.6596f, -72.8502f, 229.1833f),
    };

    public static DescentEventHandler Instance { get; private set; }

    public static NooseBonelabIntro Noose;
    public static TutorialElevator Elevator;
    public static GameControl_Descent GameController;
    public static Control_UI_BodyMeasurements BodyMeasurementsUI;

    public static Grip KnifeGrip;

    private static readonly List<DescentIntroEvent> _introEvents = new();
    private static readonly List<DescentNooseEvent> _nooseEvents = new();
    private static readonly List<DescentElevatorEvent> _elevatorEvents = new();

    public static DescentIntroEvent CreateIntroEvent(int selectionNumber, DescentIntroType type)
    {
        var value = new DescentIntroEvent(selectionNumber, type);

        if (NetworkInfo.IsHost)
        {
            _introEvents.Add(value);
        }

        return value;
    }

    public static DescentNooseEvent CreateNooseEvent(byte smallId, DescentNooseType type)
    {
        var value = new DescentNooseEvent(smallId, type);

        if (NetworkInfo.IsHost)
        {
            _nooseEvents.Add(value);
        }

        return value;
    }

    public static DescentElevatorEvent CreateElevatorEvent(DescentElevatorType type)
    {
        var value = new DescentElevatorEvent(type);

        if (NetworkInfo.IsHost)
        {
            _elevatorEvents.Add(value);
        }

        return value;
    }

    public static void CheckAchievement()
    {
        if (KnifeGrip == null && !FindKnife())
            return;

        // Check if we were holding the knife and we weren't attached to the noose
        if (!Noose.rM.IsLocalPlayer())
        {
            foreach (var hand in KnifeGrip.attachedHands)
            {
                // Make sure this is our hand
                if (hand.manager.IsLocalPlayer())
                {
                    AchievementManager.TryGetAchievement<Betrayal>(out var achievement);
                    achievement?.IncrementTask();
                    break;
                }
            }
        }
    }

    protected override void OnLevelLoaded()
    {
        base.OnLevelLoaded();

        Instance = this;
        _introEvents.Clear();

        Noose = GameObject.FindObjectOfType<NooseBonelabIntro>(true);
        Elevator = GameObject.FindObjectOfType<TutorialElevator>(true);
        GameController = GameObject.FindObjectOfType<GameControl_Descent>(true);
        BodyMeasurementsUI = GameObject.FindObjectOfType<Control_UI_BodyMeasurements>(true);
        FindKnife();
    }

    private static bool FindKnife()
    {
        var knife = GameObject.Find("SEQUENCE_EFFECTS/Dagger_A");
        if (knife != null)
        {
            KnifeGrip = knife.GetComponentInChildren<Grip>(true);
            return true;
        }

        return false;
    }

    public void CacheValues() => OnLevelLoaded();

    protected override void OnPlayerCatchup(PlayerID playerID)
    {
        // Send all intro events
        foreach (var intro in _introEvents)
        {
            MessageRelay.RelayModule<DescentIntroMessage, DescentIntroData>(new DescentIntroData() { Type = intro.Type, SelectionNumber = (byte)intro.SelectionNumber }, new MessageRoute(playerID.SmallID, NetworkChannel.Reliable));
        }

        // Send all noose events
        foreach (var noose in _nooseEvents)
        {
            MessageRelay.RelayModule<DescentNooseMessage, DescentNooseData>(new DescentNooseData() { Type = noose.Type, PlayerId = noose.PlayerId }, new MessageRoute(playerID.SmallID, NetworkChannel.Reliable));
        }

        // Send all elevator events
        foreach (var elevator in _elevatorEvents)
        {
            MessageRelay.RelayModule<DescentElevatorMessage, DescentElevatorData>(new DescentElevatorData() { Type = elevator.Type }, new MessageRoute(playerID.SmallID, NetworkChannel.Reliable));
        }
    }
}