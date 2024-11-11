using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Menu.Data;

public abstract class ElementData
{
    private string _title = "Element";
    public string Title
    {
        get
        {
            return _title;
        }
        set
        {
            _title = value;
        }
    }
}
