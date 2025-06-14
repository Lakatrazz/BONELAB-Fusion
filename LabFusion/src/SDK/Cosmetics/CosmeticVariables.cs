using LabFusion.Marrow.Integration;

namespace LabFusion.SDK.Cosmetics;

public struct CosmeticVariables
{
    public string Title;

    public string Description;

    public string Author;

    public string Category;

    public string[] Tags;

    public string Barcode;

    public int Price;

    public RigPoint CosmeticPoint;

    public bool HiddenInView;

    public bool HiddenInShop;
}