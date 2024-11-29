using LabFusion.Utilities;
using LabFusion.Scene;

using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Pool;

namespace LabFusion.Entities;

public class ConstrainerExtender : EntityComponentExtender<Constrainer>
{
    public static FusionComponentCache<Constrainer, NetworkEntity> Cache = new();

    private Grip.HandDelegate _onAttachDelegate = null;

    private Poolee _poolee = null;

    protected override void OnRegister(NetworkEntity networkEntity, Constrainer component)
    {
        Cache.Add(component, networkEntity);

        if (CrossSceneManager.IsSceneHost())
        {
            _poolee = Poolee.Cache.Get(component.gameObject);

            _onAttachDelegate = (Grip.HandDelegate)((hand) => { OnAttach(hand); });

            component.triggerGrip.attachedHandDelegate += _onAttachDelegate;
        }
    }

    protected override void OnUnregister(NetworkEntity networkEntity, Constrainer component)
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

        if (FusionDevTools.DespawnConstrainer(player.PlayerId))
        {
            _poolee.Despawn();
        }
    }
}