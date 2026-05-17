using LabFusion.Player;
using LabFusion.Senders;
using LabFusion.Utilities;

using UnityEngine;

using Il2CppSLZ.Marrow;

namespace LabFusion.SDK.Points;

using System;

[AttributeUsage(AttributeTargets.Class)]
public sealed class CompiledPointItemAttribute : Attribute { }

public sealed class PointItemUpgrade
{
    public string Description { get; }

    public int Price { get; }

    public string PurchasedDescription { get; }

    public PointItemUpgrade(string description, int price, string purchasedDescription = null)
    {
        Description = description;

        Price = price;

        if (purchasedDescription == null)
        {
            PurchasedDescription = description;
        }
        else
        {
            PurchasedDescription = purchasedDescription;
        }
    }
}