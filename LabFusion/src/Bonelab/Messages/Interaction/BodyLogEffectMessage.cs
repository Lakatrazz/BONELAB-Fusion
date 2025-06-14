using LabFusion.Entities;
using LabFusion.Network.Serialization;
using LabFusion.SDK.Modules;
using LabFusion.Network;

using Il2CppSLZ.Bonelab;

namespace LabFusion.Bonelab.Messages;

public class BodyLogEffectData : INetSerializable
{
    public const int Size = sizeof(byte);

    public byte PlayerID;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref PlayerID);
    }
}

[Net.SkipHandleWhileLoading]
public class BodyLogEffectMessage : ModuleMessageHandler
{
    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<BodyLogEffectData>();

        // Play the effect
        if (NetworkPlayerManager.TryGetPlayer(data.PlayerID, out var player) && !player.NetworkEntity.IsOwner)
        {
            PlayPullCordEffects(player);
        }
    }

    private static void PlayPullCordEffects(NetworkPlayer player)
    {
        if (!player.HasRig)
        {
            return;
        }

        var pullCord = player.RigRefs.RigManager.GetComponentInChildren<PullCordDevice>(true);

        if (pullCord == null)
        {
            return;
        }

        pullCord.PlayAvatarParticleEffects();

        pullCord._map3.PlayAtPoint(pullCord.switchAvatar, pullCord.transform.position, null, pullCord.switchVolume, 1f, new(0f), 1f, 1f);
    }
}