using LabFusion.SDK.Metadata;

namespace LabFusion.Player;

public class PlayerMetadata
{
    public NetworkMetadata Metadata { get; private set; } = null;

    public MetadataVariable Username { get; private set; } = null;
    public MetadataVariable Nickname { get; private set; } = null;
    public MetadataVariable Description { get; private set; } = null;

    public MetadataBool Loading { get; private set; } = null;
    public MetadataVariable LevelBarcode { get; private set; } = null;

    public MetadataVariable AvatarTitle { get; private set; } = null;
    public MetadataInt AvatarModID { get; private set; } = null;

    public MetadataVariable PermissionLevel { get; private set; } = null;

    public bool IsValid { get; private set; } = false;

    public void CreateMetadata()
    {
        Metadata = new NetworkMetadata();

        Username = new MetadataVariable(nameof(Username), Metadata);
        Nickname = new MetadataVariable(nameof(Nickname), Metadata);
        Description = new MetadataVariable(nameof(Description), Metadata);

        Loading = new MetadataBool(nameof(Loading), Metadata);
        LevelBarcode = new MetadataVariable(nameof(LevelBarcode), Metadata);

        AvatarTitle = new MetadataVariable(nameof(AvatarTitle), Metadata);
        AvatarModID = new MetadataInt(nameof(AvatarModID), Metadata);

        PermissionLevel = new MetadataVariable(nameof(PermissionLevel), Metadata);

        IsValid = true;
    }

    public void DestroyMetadata()
    {
        IsValid = false;

        Username.Remove();
        Nickname.Remove();
        Description.Remove();

        Loading.Remove();
        LevelBarcode.Remove();

        AvatarTitle.Remove();
        AvatarModID.Remove();

        PermissionLevel.Remove();

        Metadata = null;

        Username = null;
        Nickname = null;
        Description = null;

        Loading = null;
        LevelBarcode = null;

        AvatarTitle = null;
        AvatarModID = null;

        PermissionLevel = null;
    }
}
