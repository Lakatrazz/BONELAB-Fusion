using LabFusion.Marrow.Integration;

namespace LabFusion.SDK.Cosmetics;

public struct CosmeticVariables
{
    public string title;

    public string description;

    public string author;

    public string[] tags;

    public string barcode;

    public int price;

    public RigPoint cosmeticPoint;

    public bool hiddenInView;

    public bool hiddenInShop;
}