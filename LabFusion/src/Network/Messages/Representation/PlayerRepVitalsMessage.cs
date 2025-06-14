using LabFusion.Data;
using LabFusion.Entities;

using Il2CppSLZ.Bonelab;
using LabFusion.Network.Serialization;

namespace LabFusion.Network;

public class SerializedBodyVitals : INetSerializable
{
    public const int Size = sizeof(float) * 9 + sizeof(byte) * 5 + sizeof(int) * 2;

    public float height;
    public BodyVitals.MeasurementState measurement;
    public float chest;
    public float underbust;
    public float waist;
    public float hips;
    public float wingspan;
    public float inseam;

    public bool bodyLogFlipped;
    public bool bodyLogEnabled;
    public bool hasBodyLog;

    public bool isRightHanded;

    public int loco_CurveMode;
    public int loco_Direction;

    public SerializedBodyVitals() { }

    public SerializedBodyVitals(BodyVitals vitals)
    {
        height = vitals.realWorldHeight;
        measurement = vitals.measurementPresets;
        chest = vitals.chestCircumference;
        underbust = vitals.underbustCircumference;
        waist = vitals.waistCircumference;
        hips = vitals.hipsCircumference;
        wingspan = vitals.wingspan;

        bodyLogFlipped = vitals.bodyLogFlipped;
        bodyLogEnabled = vitals.bodyLogEnabled;
        hasBodyLog = vitals.hasBodyLog;

        isRightHanded = vitals.isRightHanded;

        loco_CurveMode = vitals.loco_CurveMode;
        loco_Direction = vitals.loco_Direction;
    }

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref height);
        serializer.SerializeValue(ref measurement, Precision.OneByte);
        serializer.SerializeValue(ref chest);
        serializer.SerializeValue(ref underbust);
        serializer.SerializeValue(ref waist);
        serializer.SerializeValue(ref hips);
        serializer.SerializeValue(ref wingspan);

        serializer.SerializeValue(ref bodyLogFlipped);
        serializer.SerializeValue(ref bodyLogEnabled);
        serializer.SerializeValue(ref hasBodyLog);

        serializer.SerializeValue(ref isRightHanded);

        serializer.SerializeValue(ref loco_CurveMode);
        serializer.SerializeValue(ref loco_Direction);
    }

    public void CopyTo(BodyVitals vitals)
    {
        vitals.realWorldHeight = height;
        vitals.measurementPresets = measurement;
        vitals.chestCircumference = chest;
        vitals.underbustCircumference = underbust;
        vitals.waistCircumference = waist;
        vitals.hipsCircumference = hips;
        vitals.wingspan = wingspan;

        vitals.bodyLogFlipped = bodyLogFlipped;
        vitals.bodyLogEnabled = bodyLogEnabled;
        vitals.hasBodyLog = hasBodyLog;

        vitals.isRightHanded = isRightHanded;

        vitals.loco_CurveMode = loco_CurveMode;
        vitals.loco_Direction = loco_Direction;

        vitals.PROPEGATE();
        vitals.CalibratePlayerBodyScale();
    }
}

public class PlayerRepVitalsData : INetSerializable
{
    public const int Size = SerializedBodyVitals.Size + sizeof(byte);

    public byte smallId;
    public SerializedBodyVitals bodyVitals;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref smallId);
        serializer.SerializeValue(ref bodyVitals);
    }

    public static PlayerRepVitalsData Create(byte smallId, BodyVitals vitals)
    {
        return new PlayerRepVitalsData()
        {
            smallId = smallId,
            bodyVitals = new SerializedBodyVitals(vitals)
        };
    }
}

public class PlayerRepVitalsMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.PlayerRepVitals;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<PlayerRepVitalsData>();

        if (NetworkPlayerManager.TryGetPlayer(data.smallId, out var player))
        {
            player.AvatarSetter.SetVitals(data.bodyVitals);
        }
    }
}