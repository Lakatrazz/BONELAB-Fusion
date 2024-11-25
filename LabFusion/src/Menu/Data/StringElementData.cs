using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Menu.Data;

public class StringElementData : ElementData
{
    private string _value = null;
    public string Value
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

    public Action<string> OnValueChanged;
}
