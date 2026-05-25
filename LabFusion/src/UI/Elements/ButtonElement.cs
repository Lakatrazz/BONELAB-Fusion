using LabFusion.Extensions;

namespace LabFusion.UI.Elements;

public class ButtonElement : TextElement
{
    public event Action Pressed;

    public void Press()
    {
        Pressed?.InvokeSafe("executing Pressed event");
    }
}
