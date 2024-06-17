using LabFusion.Utilities;

using Il2CppSLZ.Vehicle;

namespace LabFusion.Entities;

public class SeatExtender : EntityComponentArrayExtender<Seat>
{
    public static FusionComponentCache<Seat, NetworkEntity> Cache = new();

    protected override void OnRegister(NetworkEntity networkEntity, Seat[] components)
    {
        foreach (var seat in components)
        {
            Cache.Add(seat, networkEntity);
        }
    }

    protected override void OnUnregister(NetworkEntity networkEntity, Seat[] components)
    {
        foreach (var seat in components)
        {
            Cache.Remove(seat);
        }
    }
}