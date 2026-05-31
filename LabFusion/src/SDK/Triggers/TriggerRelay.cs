namespace LabFusion.SDK.Triggers;

public class TriggerRelay
{
    public delegate bool TriggerDelegate(string name);

    public delegate bool TriggerValueDelegate(string name, string value);

    // Event callbacks
    public event Action<string> Triggered;
    public event Action<string, string> TriggeredWithValue;

    // Network request callbacks
    public TriggerDelegate OnTryInvokeTrigger;
    public TriggerValueDelegate OnTryInvokeTriggerWithValue;

    public bool TryInvokeTrigger(string name)
    {
        if (OnTryInvokeTrigger == null)
        {
            return false;
        }

        return OnTryInvokeTrigger(name);
    }

    public bool TryInvokeTrigger(string name, string value)
    {
        if (OnTryInvokeTriggerWithValue == null)
        {
            return false;
        }

        return OnTryInvokeTriggerWithValue(name, value);
    }

    public void ForceInvokeLocalTrigger(string name)
    {
        Triggered?.Invoke(name);
    }

    public void ForceInvokeLocalTrigger(string name, string value)
    {
        TriggeredWithValue?.Invoke(name, value);
    }
}