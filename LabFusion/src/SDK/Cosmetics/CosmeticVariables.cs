using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LabFusion.Marrow.Integration;

using UnityEngine;

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
}