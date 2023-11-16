using BoneLib;
using BoneLib.BoneMenu;
using BoneLib.BoneMenu.Elements;

using System;

namespace LabFusion.BoneMenu
{
    public sealed class BatchedBoneMenu : IDisposable
    {
        private Action<MenuCategory, MenuElement> _action;

        private BatchedBoneMenu(Action<MenuCategory, MenuElement> action)
        {
            _action = action;
        }

        public static BatchedBoneMenu Create()
        {
            var instance = new BatchedBoneMenu(MenuCategory.OnElementCreated);
            MenuCategory.OnElementCreated = null;
            return instance;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);

            MenuCategory.OnElementCreated = _action;
            SafeActions.InvokeActionSafe(_action, MenuManager.ActiveCategory.Parent, MenuManager.ActiveCategory);
            _action = null;
        }
    }
}
