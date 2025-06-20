﻿using LabFusion.Player;
using LabFusion.Utilities;

namespace LabFusion.SDK.Achievements;

public class DaycareAttendant : Achievement
{
    public override string Title => "Daycare Attendant";

    public override string Description => "Stay in the same server for one hour.";

    public override int BitReward => 500;

    protected float _timeElapsed;
    protected bool _oneHourPassed = false;

    protected override void OnRegister()
    {
        MultiplayerHooking.OnJoinedServer += OnJoinServer;
        MultiplayerHooking.OnStartedServer += OnJoinServer;
        MultiplayerHooking.OnDisconnected += OnDisconnect;
    }

    protected override void OnUnregister()
    {
        MultiplayerHooking.OnJoinedServer -= OnJoinServer;
        MultiplayerHooking.OnStartedServer -= OnJoinServer;
        MultiplayerHooking.OnDisconnected -= OnDisconnect;

        // Incase it wasn't removed
        MultiplayerHooking.OnLateUpdate -= OnLateUpdate;
    }

    private void OnJoinServer()
    {
        _timeElapsed = 0f;
        _oneHourPassed = false;

        MultiplayerHooking.OnLateUpdate += OnLateUpdate;
    }

    private void OnDisconnect()
    {
        _timeElapsed = 0f;
        _oneHourPassed = false;

        MultiplayerHooking.OnLateUpdate -= OnLateUpdate;
    }

    private void OnLateUpdate()
    {
        // If we haven't already given the achievement, and there is more than 1 player, increment the timer
        if (!_oneHourPassed && PlayerIDManager.HasOtherPlayers)
        {
            _timeElapsed += TimeUtilities.DeltaTime;

            // 3600 seconds in an hour
            if (_timeElapsed >= 3600f)
            {
                _oneHourPassed = true;
                IncrementTask();
            }
        }
    }
}
