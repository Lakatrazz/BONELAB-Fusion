using LabFusion.Network.Serialization;
using LabFusion.Preferences.Client;

using UnityEngine;

namespace LabFusion.Data;

public class SerializedPlayerSettings : INetSerializable
{
    public const int Size = sizeof(float) * 4;

    public Color nametagColor;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref nametagColor);
    }

    public static SerializedPlayerSettings Create()
    {
        var settings = new SerializedPlayerSettings()
        {
            nametagColor = ClientSettings.NameTagColor,
        };

        return settings;
    }
}