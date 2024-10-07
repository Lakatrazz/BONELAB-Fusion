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
        writer.Write(settings.VoiceChatEnabled.Value);
        writer.Write(settings.PlayerConstraintsEnabled.Value);
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
            NametagsEnabled = new ReadOnlyPref<bool>(reader.ReadBoolean()),
            VoiceChatEnabled = new ReadOnlyPref<bool>(reader.ReadBoolean()),
            PlayerConstraintsEnabled = new ReadOnlyPref<bool>(reader.ReadBoolean()),
            Privacy = new ReadOnlyPref<ServerPrivacy>((ServerPrivacy)reader.ReadByte()),
            TimeScaleMode = new ReadOnlyPref<TimeScaleMode>((TimeScaleMode)reader.ReadByte()),
            MaxPlayers = new ReadOnlyPref<int>(reader.ReadInt32()),

            // Visual
            ServerName = new ReadOnlyPref<string>(reader.ReadString()),
            ServerTags = new ReadOnlyPref<List<string>>(reader.ReadStrings().ToList()),

            // Mortality
            ServerMortality = new ReadOnlyPref<bool>(reader.ReadBoolean()),

            // Server permissions
            DevToolsAllowed = new ReadOnlyPref<PermissionLevel>((PermissionLevel)reader.ReadSByte()),
            ConstrainerAllowed = new ReadOnlyPref<PermissionLevel>((PermissionLevel)reader.ReadSByte()),
            CustomAvatarsAllowed = new ReadOnlyPref<PermissionLevel>((PermissionLevel)reader.ReadSByte()),
            KickingAllowed = new ReadOnlyPref<PermissionLevel>((PermissionLevel)reader.ReadSByte()),
            BanningAllowed = new ReadOnlyPref<PermissionLevel>((PermissionLevel)reader.ReadSByte()),
            Teleportation = new ReadOnlyPref<PermissionLevel>((PermissionLevel)reader.ReadSByte()),
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