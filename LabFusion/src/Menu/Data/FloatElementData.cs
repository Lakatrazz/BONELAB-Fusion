using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Menu.Data;

public class FloatElementData : ElementData
{
    private float _value = 0;
    public float Value
    {
        get
        {
            return _value;
        }
        set
        {
            _value = value;

            OnValueChanged?.Invoke(value);
        }
    }

    public float MinValue { get; set; } = 0f;

    public float MaxValue { get; set; } = 1f;

    public float Increment { get; set; } = 0.01f;

    public Action<float> OnValueChanged;
}
