using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.UI.Elements;

public class FloatField : FieldElement<float>
{
    public float? MinValue { get; set; } = null;

    public float? MaxValue { get; set; } = null;

    public float Increment { get; set; } = 1f;
}
