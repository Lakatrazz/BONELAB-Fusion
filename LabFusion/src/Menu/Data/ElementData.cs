using UnityEngine;

namespace LabFusion.Menu.Data;

public abstract class ElementData
{
    public string Title { get; set; } = "Element";

    public Color Color { get; set; } = Color.white;
}