using LabFusion.Data;
using LabFusion.Entities;

using Il2CppSLZ.Bonelab;

namespace LabFusion.Network;

public class SerializedBodyVitals : IFusionSerializable
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

    public void Serialize(FusionWriter writer)
    {
        writer.Write(height);
        writer.Write((byte)measurement);
        writer.Write(chest);
        writer.Write(underbust);
        writer.Write(waist);
        writer.Write(hips);
        writer.Write(wingspan);

        writer.Write(bodyLogFlipped);
        writer.Write(bodyLogEnabled);
        writer.Write(hasBodyLog);

        writer.Write(isRightHanded);

        writer.Write(loco_CurveMode);
        writer.Write(loco_Direction);
    }

    public void Deserialize(FusionReader reader)
    {
        height = reader.ReadSingle();
        measurement = (BodyVitals.MeasurementState)reader.ReadByte();
        chest = reader.ReadSingle();
        underbust = reader.ReadSingle();
        waist = reader.ReadSingle();
        hips = reader.ReadSingle();
        wingspan = reader.ReadSingle();

        bodyLogFlipped = reader.ReadBoolean();
        bodyLogEnabled = reader.ReadBoolean();
        hasBodyLog = reader.ReadBoolean();

        isRightHanded = reader.ReadBoolean();

        loco_CurveMode = reader.ReadInt32();
        loco_Direction = reader.ReadInt32();
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

public class PlayerRepVitalsData : IFusionSerializable
{
    public const int Size = SerializedBodyVitals.Size + sizeof(byte);

    public byte smallId;
    public SerializedBodyVitals bodyVitals;

    public void Serialize(FusionWriter writer)
    {
        writer.Write(smallId);
        writer.Write(bodyVitals);
    }

    public void Deserialize(FusionReader reader)
    {
        smallId = reader.ReadByte();
        bodyVitals = reader.ReadFusionSerializable<SerializedBodyVitals>();
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

public class PlayerRepVitalsMessage : FusionMessageHandler
{
    public override byte? Tag => NativeMessageTag.PlayerRepVitals;

    public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
    {
        using var reader = FusionReader.Create(bytes);
        var data = reader.ReadFusionSerializable<PlayerRepVitalsData>();

        if (NetworkPlayerManager.TryGetPlayer(data.smallId, out var player))
        {
            player.AvatarSetter.SetVitals(data.bodyVitals);
        }

        if (NetworkInfo.IsServer)
        {
            using var message = FusionMessage.Create(Tag.Value, bytes);
            MessageSender.BroadcastMessageExcept(data.smallId, NetworkChannel.Reliable, message);
        }
    }
}