using LabFusion.Network;
using LabFusion.Preferences;
using LabFusion.Senders;
using LabFusion.Representation;

namespace LabFusion.Data;

public class SerializedServerSettings : IFusionSerializable
{
    public const int Size = sizeof(byte) * 13;

    public ServerSettings settings;

    public void Serialize(FusionWriter writer)
    {
        // General settings
        writer.Write(settings.NametagsEnabled.Value);
        writer.Write(settings.VoicechatEnabled.Value);
        writer.Write(settings.PlayerConstraintsEnabled.Value);
        writer.Write(settings.VoteKickingEnabled.Value);
        writer.Write((byte)settings.Privacy.Value);
        writer.Write((byte)settings.TimeScaleMode.Value);
        writer.Write(settings.MaxPlayers.Value);

        // Visual
        writer.Write(settings.ServerName.Value);
        writer.Write(settings.ServerTags.Value);

        // Mortality
        writer.Write(settings.ServerMortality.Value);

        // Server permissions
        writer.Write((sbyte)settings.DevToolsAllowed.Value);
        writer.Write((sbyte)settings.ConstrainerAllowed.Value);
        writer.Write((sbyte)settings.CustomAvatarsAllowed.Value);
        writer.Write((sbyte)settings.KickingAllowed.Value);
        writer.Write((sbyte)settings.BanningAllowed.Value);
        writer.Write((sbyte)settings.Teleportation.Value);
    }

    public void Deserialize(FusionReader reader)
    {
        settings = new ServerSettings
        {
            // General settings
            NametagsEnabled = new ReadonlyPref<bool>(reader.ReadBoolean()),
            VoicechatEnabled = new ReadonlyPref<bool>(reader.ReadBoolean()),
            PlayerConstraintsEnabled = new ReadonlyPref<bool>(reader.ReadBoolean()),
            VoteKickingEnabled = new ReadonlyPref<bool>(reader.ReadBoolean()),
            Privacy = new ReadonlyPref<ServerPrivacy>((ServerPrivacy)reader.ReadByte()),
            TimeScaleMode = new ReadonlyPref<TimeScaleMode>((TimeScaleMode)reader.ReadByte()),
            MaxPlayers = new ReadonlyPref<byte>(reader.ReadByte()),

            // Visual
            ServerName = new ReadonlyPref<string>(reader.ReadString()),
            ServerTags = new ReadonlyPref<List<string>>(reader.ReadStrings().ToList()),

            // Mortality
            ServerMortality = new ReadonlyPref<bool>(reader.ReadBoolean()),

            // Server permissions
            DevToolsAllowed = new ReadonlyPref<PermissionLevel>((PermissionLevel)reader.ReadSByte()),
            ConstrainerAllowed = new ReadonlyPref<PermissionLevel>((PermissionLevel)reader.ReadSByte()),
            CustomAvatarsAllowed = new ReadonlyPref<PermissionLevel>((PermissionLevel)reader.ReadSByte()),
            KickingAllowed = new ReadonlyPref<PermissionLevel>((PermissionLevel)reader.ReadSByte()),
            BanningAllowed = new ReadonlyPref<PermissionLevel>((PermissionLevel)reader.ReadSByte()),
            Teleportation = new ReadonlyPref<PermissionLevel>((PermissionLevel)reader.ReadSByte()),
        };
    }

    public static SerializedServerSettings Create()
    {
        var settings = new SerializedServerSettings()
        {
            settings = ServerSettingsManager.SavedSettings,
        };

        return settings;
    }
}