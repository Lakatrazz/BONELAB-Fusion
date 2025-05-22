using LabFusion.Network;

namespace LabFusion.SDK.Triggers;

public class TriggerEvent
{
    public TriggerRelay Relay { get; }
    public string Name { get; }
    public bool ServerOnly { get; }

    public event Action OnTriggered;
    public event Action<string> OnTriggeredWithValue;

    public TriggerEvent(string name, TriggerRelay relay, bool serverOnly = false)
    {
        Name = name;
        Relay = relay;
        ServerOnly = serverOnly;

        relay.OnTriggered += OnRelayTriggered;
        relay.OnTriggeredWithValue += OnRelayTriggeredWithValue;
    }

    public void UnregisterEvent()
    {
        // Unhook from relay
        Relay.OnTriggered -= OnRelayTriggered;
        Relay.OnTriggeredWithValue -= OnRelayTriggeredWithValue;

        // Remove trigger hooks
        OnTriggered = null;
        OnTriggeredWithValue = null;
    }

    private void OnRelayTriggered(string name)
    {
        if (Name != name)
        {
            return;
        }

        OnTriggered?.Invoke();
    }

    private void OnRelayTriggeredWithValue(string name, string value)
    {
        if (Name != name)
        {
            return;
        }

        OnTriggeredWithValue?.Invoke(value);
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
        if (ServerOnly && !NetworkInfo.IsHost)
        {
            return false;
        }

        return true;
    }
}