using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.UI.Elements;

public class IntField : FieldElement<int>
{
    public int? MinValue { get; set; } = null;

    public int? MaxValue { get; set; } = null;

    public int Increment { get; set; } = 1;
}
