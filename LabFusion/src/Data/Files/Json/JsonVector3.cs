using System.Text.Json.Serialization;

using UnityEngine;

namespace LabFusion.Data;

[Serializable]
public struct JsonVector3
{
    [JsonPropertyName("x")]
    public float X { get; set; }

    [JsonPropertyName("y")]
    public float Y { get; set; }

    [JsonPropertyName("z")]
    public float Z { get; set; }

    public JsonVector3() : this(0f, 0f, 0f) { }

    public JsonVector3(float x, float y, float z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public JsonVector3(Vector3 vector3) : this(vector3.x, vector3.y, vector3.z) { }

    public readonly Vector3 ToUnityVector3()
    {
        return new Vector3(X, Y, Z);
    }
}
