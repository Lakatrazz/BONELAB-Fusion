using BoneLib;
using BoneLib.BoneMenu;

namespace LabFusion.BoneMenu;

public sealed class BatchedBoneMenu : IDisposable
{
    private Action<Page> _action;

    private BatchedBoneMenu(Action<Page> action)
    {
        _action = action;
    }

    public static BatchedBoneMenu Create()
    {
        var instance = new BatchedBoneMenu(Menu.OnPageUpdated);
        Menu.OnPageUpdated = null;
        return instance;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        Menu.OnPageUpdated = _action;
        SafeActions.InvokeActionSafe(_action, Menu.CurrentPage);
        _action = null;
    }
}