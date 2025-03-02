using LabFusion.Network;
using LabFusion.Utilities;
using LabFusion.Math;

using Il2CppSLZ.Marrow.Input;
using Il2CppSLZ.Marrow;

using UnityEngine;

using LabFusion.Network.Serialization;

namespace LabFusion.Data;

public class SerializedController : INetSerializable
{
    public const int Size = sizeof(float) * 3 + sizeof(byte) * 12;

    public float indexCurl;
    public float middleCurl;
    public float ringCurl;
    public float pinkyCurl;
    public float thumbCurl;

    public float solvedGrip;
    public float primaryAxis;

    public XRControllerType controllerType;
    public BaseController.GesturePose gesturePose;
    public float gesturePoseIntensity;

    public bool primaryInteractionButton;
    public bool secondaryInteractionButton;

    public Vector2 thumbstickAxis;

    public const float PRECISION_MULTIPLIER = 255f;

    public SerializedController() { }

    public SerializedController(BaseController controller)
    {
        indexCurl = controller._processedIndex;
        middleCurl = controller._processedMiddle;
        ringCurl = controller._processedRing;
        pinkyCurl = controller._processedPinky;
        thumbCurl = controller._processedThumb;

        solvedGrip = controller._solvedGrip;
        primaryAxis = controller._primaryAxis;

        controllerType = controller.Type;
        gesturePose = controller._gesturePose;
        gesturePoseIntensity = controller._gesturePoseIntensity;

        primaryInteractionButton = controller._primaryInteractionButton;
        secondaryInteractionButton = controller._secondaryInteractionButton;

        thumbstickAxis = controller._thumbstickAxis;
    }

    public void CopyTo(BaseController controller)
    {
        controller._processedIndex = indexCurl;
        controller._processedMiddle = middleCurl;
        controller._processedRing = ringCurl;
        controller._processedPinky = pinkyCurl;
        controller._processedThumb = thumbCurl;

        controller._solvedGrip = ManagedMathf.Clamp(solvedGrip, 0f, OpenController.grabThreshold - 0.01f);
        controller._primaryAxis = primaryAxis;

        controller.Type = controllerType;
        controller._gesturePose = gesturePose;
        controller._gesturePoseIntensity = gesturePoseIntensity;

        // Primary interaction button
        bool primaryUp = controller._primaryInteractionButtonUp;
        bool primaryDown = controller._primaryInteractionButtonDown;

        SolveButtonPress(controller._primaryInteractionButton, primaryInteractionButton, ref primaryUp, ref primaryDown);

        controller._primaryInteractionButtonUp = primaryUp;
        controller._primaryInteractionButtonDown = primaryDown;

        controller._primaryInteractionButton = primaryInteractionButton;

        // Secondary interaction button
        bool secondaryUp = controller._secondaryInteractionButtonUp;
        bool secondaryDown = controller._secondaryInteractionButtonDown;

        SolveButtonPress(controller._secondaryInteractionButton, secondaryInteractionButton, ref secondaryUp, ref secondaryDown);

        controller._secondaryInteractionButtonUp = secondaryUp;
        controller._secondaryInteractionButtonDown = secondaryDown;

        controller._secondaryInteractionButton = secondaryInteractionButton;

        // Thumbstick
        controller._thumbstickAxis = thumbstickAxis;

        // Update hovering so that grips solve properly
        controller._lastTimeGrabbed = TimeUtilities.TimeSinceStartup;
    }

    public static void SolveButtonPress(bool lastValue, bool newValue, ref bool up, ref bool down)
    {
        if (newValue)
        {
            up = false;

            if (!lastValue)
                down = true;
            else
                down = false;
        }
        else
        {
            down = false;

            if (lastValue)
                up = true;
            else
                up = false;
        }
    }

    public void Serialize(INetSerializer serializer)
    {
        SerializeCompressedFloat(serializer, ref indexCurl);
        SerializeCompressedFloat(serializer, ref middleCurl);
        SerializeCompressedFloat(serializer, ref ringCurl);
        SerializeCompressedFloat(serializer, ref pinkyCurl);
        SerializeCompressedFloat(serializer, ref thumbCurl);

        SerializeCompressedFloat(serializer, ref solvedGrip);
        SerializeCompressedFloat(serializer, ref primaryAxis);

        serializer.SerializeValue(ref controllerType, Precision.OneByte);
        serializer.SerializeValue(ref gesturePose, Precision.OneByte);
        SerializeCompressedFloat(serializer, ref gesturePoseIntensity);

        serializer.SerializeValue(ref primaryInteractionButton);
        serializer.SerializeValue(ref secondaryInteractionButton);

        SerializedSmallDirection2D thumbstickAxis = null;

        if (!serializer.IsReader)
        {
            thumbstickAxis = SerializedSmallDirection2D.Compress(this.thumbstickAxis);
        }

        serializer.SerializeValue(ref thumbstickAxis);

        if (serializer.IsReader)
        {
            this.thumbstickAxis = thumbstickAxis.Expand();
        }
    }

    private static void SerializeCompressedFloat(INetSerializer serializer, ref float value)
    {
        byte compressed = 0;

        if (!serializer.IsReader)
        {
            compressed = (byte)(value * PRECISION_MULTIPLIER);
        }

        serializer.SerializeValue(ref compressed);

        if (serializer.IsReader)
        {
            value = ((float)compressed) / PRECISION_MULTIPLIER;
        }
    }
}