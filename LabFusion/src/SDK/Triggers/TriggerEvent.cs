using LabFusion.Network;

namespace LabFusion.SDK.Triggers;

public class TriggerEvent
{
    public TriggerRelay Relay { get; }
    public string Name { get; }
    public bool ServerOnly { get; }

    public event Action Triggered;
    public event Action<string> TriggeredWithValue;

    public TriggerEvent(string name, TriggerRelay relay, bool serverOnly = false)
    {
        Name = name;
        Relay = relay;
        ServerOnly = serverOnly;

        relay.Triggered += OnRelayTriggered;
        relay.TriggeredWithValue += OnRelayTriggeredWithValue;
    }

    public void UnregisterEvent()
    {
        // Unhook from relay
        Relay.Triggered -= OnRelayTriggered;
        Relay.TriggeredWithValue -= OnRelayTriggeredWithValue;

        // Remove trigger hooks
        Triggered = null;
        TriggeredWithValue = null;
    }

    private void OnRelayTriggered(string name)
    {
        if (Name != name)
        {
            return;
        }

        Triggered?.Invoke();
    }

    private void OnRelayTriggeredWithValue(string name, string value)
    {
        if (Name != name)
        {
            return;
        }

        TriggeredWithValue?.Invoke(value);
    }

    public bool TryInvoke()
    {
        if (!CanInvoke())
        {
            return false;
        }

        return Relay.TryInvokeTrigger(Name);
    }

    public bool TryInvoke(string value)
    {
        if (!CanInvoke())
        {
            return false;
        }

        return Relay.TryInvokeTrigger(Name, value);
    }

    public bool CanInvoke()
    {
        if (ServerOnly && !ServerManager.IsServerRunning)
        {
            return false;
        }

        return true;
    }
}