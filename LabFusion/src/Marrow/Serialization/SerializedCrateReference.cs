using Il2CppSLZ.Marrow.Warehouse;

using LabFusion.Network.Serialization;

namespace LabFusion.Marrow.Serialization;

public struct SerializedCrateReference : INetSerializable
{
    public static readonly SerializedCrateReference None = new()
    {
        Barcode = string.Empty,
        Title = string.Empty,
    };

    public string Barcode;

    public string Title;

    public readonly bool IsValid => !string.IsNullOrWhiteSpace(Barcode);

    public readonly int? GetSize() => Barcode.GetSize();

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref Barcode);
        serializer.SerializeValue(ref Title);
    }

    public readonly bool HasCrate() => AssetWarehouseSearcher.HasCrate(new(Barcode));

    public readonly bool HasCrate<TCrate>() where TCrate : Crate => AssetWarehouseSearcher.HasCrate<TCrate>(new(Barcode));

    public readonly Crate GetCrate() => AssetWarehouseSearcher.GetCrate(new(Barcode));

    public readonly TCrate GetCrate<TCrate>() where TCrate : Crate => AssetWarehouseSearcher.GetCrate<TCrate>(new(Barcode));

    public SerializedCrateReference(string barcode) : this(new Barcode(barcode)) { }

    public SerializedCrateReference(Barcode barcode)
    {
        Barcode = barcode.ID;

        var crate = AssetWarehouseSearcher.GetCrate(barcode);

        if (crate != null)
        {
            Title = crate.Title;
        }
        else
        {
            Title = Barcode;
        }
    }
}
