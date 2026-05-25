namespace LabFusion.UI.Elements;

public interface IRepaintNotifier
{
    event Action Repainted;

    void Repaint();
}
