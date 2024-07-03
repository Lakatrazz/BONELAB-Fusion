using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.SDK.Triggers;

public class TriggerRelay
{
    public delegate bool TriggerDelegate(string name);

    public delegate bool TriggerValueDelegate(string name, string value);

    // Event callbacks
    public event Action<string> OnTriggered;
    public event Action<string, string> OnTriggeredWithValue;

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
        OnTriggered?.Invoke(name);
    }

    public void ForceInvokeLocalTrigger(string name, string value)
    {
        OnTriggeredWithValue?.Invoke(name, value);
    }
}