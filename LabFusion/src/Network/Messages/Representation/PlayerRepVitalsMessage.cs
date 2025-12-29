using LabFusion.Entities;
using LabFusion.Network.Serialization;

using Il2CppSLZ.Bonelab;

namespace LabFusion.Network;

public class SerializedBodyVitals : INetSerializable
{
    public const int Size = sizeof(float) * 9 + sizeof(byte) * 5 + sizeof(int) * 2;

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

    public int Loco_CurveMode;
    public int Loco_Direction;

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

        serializer.SerializeValue(ref Loco_CurveMode);
        serializer.SerializeValue(ref Loco_Direction);
    }

    public void CopyTo(BodyVitals vitals)
    {
        vitals.realWorldHeight = Height;
        vitals.measurementPresets = Measurement;
        vitals.chestCircumference = Chest;
        vitals.underbustCircumference = Underbust;
        vitals.waistCircumference = Waist;
        vitals.hipsCircumference = Hips;
        vitals.wingspan = Wingspan;

        vitals.bodyLogFlipped = BodyLogFlipped;
        vitals.bodyLogEnabled = BodyLogEnabled;
        vitals.hasBodyLog = HasBodyLog;

        vitals.isRightHanded = IsRightHanded;

        vitals.loco_CurveMode = Loco_CurveMode;
        vitals.loco_Direction = Loco_Direction;

        vitals.PROPEGATE();
        vitals.CalibratePlayerBodyScale();
    }
}

public class PlayerRepVitalsData : INetSerializable
{
    public const int Size = SerializedBodyVitals.Size;

    public SerializedBodyVitals BodyVitals;

    public int? GetSize() => Size;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref BodyVitals);
    }
}

public class PlayerRepVitalsMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.PlayerRepVitals;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<PlayerRepVitalsData>();

        var sender = received.Sender;

        if (!sender.HasValue)
        {
            return;
        }

        if (NetworkPlayerManager.TryGetPlayer(sender.Value, out var player))
        {
            player.AvatarSetter.SetVitals(data.BodyVitals);
        }
    }
}