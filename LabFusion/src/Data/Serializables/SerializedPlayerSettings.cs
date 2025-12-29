using LabFusion.Network.Serialization;
using LabFusion.Preferences.Client;

using UnityEngine;

namespace LabFusion.Data;

public class SerializedPlayerSettings : INetSerializable
{
    public const int Size = sizeof(float) * 4;

    public Color NametagColor;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref NametagColor);
    }

    public static SerializedPlayerSettings Create()
    {
        var settings = new SerializedPlayerSettings()
        {
            NametagColor = ClientSettings.NameTagColor,
        };

        return settings;
    }
}