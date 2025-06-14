using LabFusion.Utilities;
using LabFusion.Scene;

using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Pool;

namespace LabFusion.Entities;

public class ConstrainerExtender : EntityComponentExtender<Constrainer>
{
    public static readonly FusionComponentCache<Constrainer, NetworkEntity> Cache = new();

    private Grip.HandDelegate _onAttachDelegate = null;

    private Poolee _poolee = null;

    protected override void OnRegister(NetworkEntity entity, Constrainer component)
    {
        Cache.Add(component, entity);

        if (NetworkSceneManager.IsLevelHost)
        {
            _poolee = Poolee.Cache.Get(component.gameObject);

            _onAttachDelegate = (Grip.HandDelegate)((hand) => { OnAttach(hand); });

            component.triggerGrip.attachedHandDelegate += _onAttachDelegate;
        }
    }

    protected override void OnUnregister(NetworkEntity entity, Constrainer component)
    {
        Cache.Remove(component);

        if (_onAttachDelegate != null)
        {
            component.triggerGrip.attachedHandDelegate -= _onAttachDelegate;

            _onAttachDelegate = null;
            _poolee = null;
        }
    }

    private void OnAttach(Hand hand)
    {
        if (_poolee == null)
        {
            return;
        }

        var manager = hand.manager;

        if (!NetworkPlayerManager.TryGetPlayer(manager, out var player))
        {
            return;
        }

        if (FusionDevTools.DespawnConstrainer(player.PlayerID))
        {
            _poolee.Despawn();
        }
    }
}