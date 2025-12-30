using Il2CppSLZ.Bonelab;

using LabFusion.Network.Serialization;

namespace LabFusion.Bonelab.Data;

public class SerializedBodyVitals : INetSerializable
{
    public const int Size = sizeof(float) * 8 + sizeof(byte) * 5 + sizeof(int) * 3;

    public float Height;
    public BodyVitals.MeasurementState Measurement;
    public float Chest;
    public float Underbust;
    public float Waist;
    public float Hips;
    public float Wingspan;
    public float Inseam;

    public bool BodyLogFlipped;
    public bool BodyLogEnabled;
    public bool HasBodyLog;

    public bool IsRightHanded;

    public float Loco_DegreesPerSnap;
    public int Loco_SnapDegreesPerFrame;
    public int Loco_CurveMode;
    public int Loco_Direction;

    public int? GetSize() => Size;

    public SerializedBodyVitals() { }

    public SerializedBodyVitals(BodyVitals vitals)
    {
        Height = vitals.realWorldHeight;
        Measurement = vitals.measurementPresets;
        Chest = vitals.chestCircumference;
        Underbust = vitals.underbustCircumference;
        Waist = vitals.waistCircumference;
        Hips = vitals.hipsCircumference;
        Wingspan = vitals.wingspan;

        BodyLogFlipped = vitals.bodyLogFlipped;
        BodyLogEnabled = vitals.bodyLogEnabled;
        HasBodyLog = vitals.hasBodyLog;

        IsRightHanded = vitals.isRightHanded;

        Loco_DegreesPerSnap = vitals.loco_DegreesPerSnap;
        Loco_SnapDegreesPerFrame = vitals.loco_SnapDegreesPerFrame;
        Loco_CurveMode = vitals.loco_CurveMode;
        Loco_Direction = vitals.loco_Direction;
    }

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref Height);
        serializer.SerializeValue(ref Measurement, Precision.OneByte);
        serializer.SerializeValue(ref Chest);
        serializer.SerializeValue(ref Underbust);
        serializer.SerializeValue(ref Waist);
        serializer.SerializeValue(ref Hips);
        serializer.SerializeValue(ref Wingspan);

        serializer.SerializeValue(ref BodyLogFlipped);
        serializer.SerializeValue(ref BodyLogEnabled);
        serializer.SerializeValue(ref HasBodyLog);

        serializer.SerializeValue(ref IsRightHanded);

        serializer.SerializeValue(ref Loco_DegreesPerSnap);
        serializer.SerializeValue(ref Loco_SnapDegreesPerFrame);
        serializer.SerializeValue(ref Loco_CurveMode);
        serializer.SerializeValue(ref Loco_Direction);
    }
}
