using LabFusion.Extensions;

using UnityEngine;

namespace LabFusion.Scene;

public static class NetworkTransformManager
{
    public static Vector3 FloatingOrigin { get; set; } = Vector3Extensions.zero;

    public static Vector3 EncodePosition(this Vector3 position)
    {
        return position - FloatingOrigin;
    }

    public static Vector3 DecodePosition(this Vector3 position) 
    { 
        return position + FloatingOrigin; 
    }
}
