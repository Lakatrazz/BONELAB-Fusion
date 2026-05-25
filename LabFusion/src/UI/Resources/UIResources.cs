using Il2CppTMPro;

using LabFusion.Menu;

using UnityEngine;

namespace LabFusion.UI.Resources;

public static class UIResources
{
    /// <summary>
    /// Gets a built-in icon by name.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static Texture GetCommonIcon(string name) => MenuResources.GetCommonIcon(name);

    /// <summary>
    /// Gets a built-in font asset by name.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static TMP_FontAsset GetCommonFont(string name) => MenuResources.GetCommonFont(name);
}
