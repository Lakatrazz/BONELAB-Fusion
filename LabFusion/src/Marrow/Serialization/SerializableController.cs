using LabFusion.Utilities;
using LabFusion.Math;
using LabFusion.Network.Serialization;
using LabFusion.Data;

using Il2CppSLZ.Marrow.Input;
using Il2CppSLZ.Marrow;

using UnityEngine;

namespace LabFusion.Marrow.Serialization;

public class SerializableController : INetSerializable
{
    public const int Size = sizeof(float) * 3 + sizeof(byte) * 12;

    public float IndexCurl;
    public float MiddleCurl;
    public float RingCurl;
    public float PinkyCurl;
    public float ThumbCurl;

    public float SolvedGrip;
    public float PrimaryAxis;

    public XRControllerType ControllerType;
    public BaseController.GesturePose GesturePose;
    public float GesturePoseIntensity;

    public bool PrimaryInteractionButton;
    public bool SecondaryInteractionButton;

    public Vector2 ThumbstickAxis;

    public const float PRECISION_MULTIPLIER = 255f;

    public SerializableController() { }

    public SerializableController(BaseController controller)
    {
        IndexCurl = controller._processedIndex;
        MiddleCurl = controller._processedMiddle;
        RingCurl = controller._processedRing;
        PinkyCurl = controller._processedPinky;
        ThumbCurl = controller._processedThumb;

        SolvedGrip = controller._solvedGrip;
        PrimaryAxis = controller._primaryAxis;

        ControllerType = controller.Type;
        GesturePose = controller._gesturePose;
        GesturePoseIntensity = controller._gesturePoseIntensity;

        PrimaryInteractionButton = controller._primaryInteractionButton;
        SecondaryInteractionButton = controller._secondaryInteractionButton;

        ThumbstickAxis = controller._thumbstickAxis;
    }

    public void CopyTo(BaseController controller)
    {
        controller._processedIndex = IndexCurl;
        controller._processedMiddle = MiddleCurl;
        controller._processedRing = RingCurl;
        controller._processedPinky = PinkyCurl;
        controller._processedThumb = ThumbCurl;

        controller._solvedGrip = ManagedMathf.Clamp(SolvedGrip, 0f, OpenController.grabThreshold - 0.01f);
        controller._primaryAxis = PrimaryAxis;

        controller.Type = ControllerType;
        controller._gesturePose = GesturePose;
        controller._gesturePoseIntensity = GesturePoseIntensity;

        // Primary interaction button
        bool primaryUp = controller._primaryInteractionButtonUp;
        bool primaryDown = controller._primaryInteractionButtonDown;

        SolveButtonPress(controller._primaryInteractionButton, PrimaryInteractionButton, ref primaryUp, ref primaryDown);

        controller._primaryInteractionButtonUp = primaryUp;
        controller._primaryInteractionButtonDown = primaryDown;

        controller._primaryInteractionButton = PrimaryInteractionButton;

        // Secondary interaction button
        bool secondaryUp = controller._secondaryInteractionButtonUp;
        bool secondaryDown = controller._secondaryInteractionButtonDown;

        SolveButtonPress(controller._secondaryInteractionButton, SecondaryInteractionButton, ref secondaryUp, ref secondaryDown);

        controller._secondaryInteractionButtonUp = secondaryUp;
        controller._secondaryInteractionButtonDown = secondaryDown;

        controller._secondaryInteractionButton = SecondaryInteractionButton;

        // Thumbstick
        controller._thumbstickAxis = ThumbstickAxis;

        // Update hovering so that grips solve properly
        controller._lastTimeGrabbed = TimeUtilities.TimeSinceStartup;
    }

    public static void SolveButtonPress(bool lastValue, bool newValue, ref bool up, ref bool down)
    {
        if (newValue)
        {
            up = false;

            if (!lastValue)
            {
                down = true;
            }
            else
            {
                down = false;
            }
        }
        else
        {
            down = false;

            if (lastValue)
            {
                up = true;
            }
            else
            {
                up = false;
            }
        }
    }

    public void Serialize(INetSerializer serializer)
    {
        SerializeCompressedFloat(serializer, ref IndexCurl);
        SerializeCompressedFloat(serializer, ref MiddleCurl);
        SerializeCompressedFloat(serializer, ref RingCurl);
        SerializeCompressedFloat(serializer, ref PinkyCurl);
        SerializeCompressedFloat(serializer, ref ThumbCurl);

        SerializeCompressedFloat(serializer, ref SolvedGrip);
        SerializeCompressedFloat(serializer, ref PrimaryAxis);

        serializer.SerializeValue(ref ControllerType, Precision.OneByte);
        serializer.SerializeValue(ref GesturePose, Precision.OneByte);
        SerializeCompressedFloat(serializer, ref GesturePoseIntensity);

        serializer.SerializeValue(ref PrimaryInteractionButton);
        serializer.SerializeValue(ref SecondaryInteractionButton);

        SerializedSmallDirection2D thumbstickAxis = null;

        if (!serializer.IsReader)
        {
            thumbstickAxis = SerializedSmallDirection2D.Compress(this.ThumbstickAxis);
        }

        serializer.SerializeValue(ref thumbstickAxis);

        if (serializer.IsReader)
        {
            this.ThumbstickAxis = thumbstickAxis.Expand();
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