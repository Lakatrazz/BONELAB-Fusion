using LabFusion.Player;
using LabFusion.Utilities;
using LabFusion.Extensions;
using LabFusion.Network;

namespace LabFusion.SDK.Metadata;

public class MetadataPlayerDictionary<TVariable> : MetadataDictionary<byte, TVariable> where TVariable : MetadataVariable
{
    public event Action<PlayerID, TVariable> OnPlayerVariableChanged;

    protected override void OnRegistered()
    {
        OnVariableChanged += OnByteVariableChanged;

        MultiplayerHooking.OnPlayerLeft += OnPlayerLeft;
    }

    protected override void OnUnregistered()
    {
        OnVariableChanged -= OnByteVariableChanged;

        MultiplayerHooking.OnPlayerLeft -= OnPlayerLeft;
    }

    private void OnByteVariableChanged(byte smallID, TVariable variable)
    {
        var playerID = PlayerIDManager.GetPlayerID(new ClientSmallID(smallID));

        if (playerID != null)
        {
            OnPlayerVariableChanged?.InvokeSafe(playerID, variable, "executing MetadataPlayerDictionary.OnPlayerVariableChanged");
        }
    }

    private void OnPlayerLeft(PlayerID playerID)
    {
        RemoveVariable(playerID);
    }

    public override string GetKeyWithProperty(byte property)
    {
        return KeyHelper.GetKeyFromPlayer(Key, new ClientSmallID(property));
    }

    public override byte GetPropertyWithKey(string key)
    {
        return (byte)KeyHelper.GetPlayerFromKey(key);
    }

    public TVariable GetVariable(PlayerID playerID) => GetVariable((byte)playerID.SmallID);

    public void RemoveVariable(PlayerID playerID) => RemoveVariable((byte)playerID.SmallID);
}
